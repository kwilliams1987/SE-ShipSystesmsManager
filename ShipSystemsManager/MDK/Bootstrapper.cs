﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using IngameScript.Mockups;
using IngameScript.Mockups.Asserts;
using IngameScript.Mockups.Blocks;
using Malware.MDKUtilities;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI;
using VRageMath;

namespace IngameScript.MDK
{
    public class TestBootstrapper
    {
        static class Styler
        {
            public static T Get<T>(String key) => (T)Program.BaseStyler.DefaultStyles[key];
        }
        
        // All the files in this folder, as well as all files containing the file ".debug.", will be excluded
        // from the build process. You can use this to create utilites for testing your scripts directly in 
        // Visual Studio.

        static TestBootstrapper()
        {
            // Initialize the MDK utility framework
            MDKUtilityFramework.Load();
        }

        public static void Main()
        {
            // In order for your program to actually run, you will need to provide a mockup of all the facilities 
            // your script uses from the game, since they're not available outside of the game.

            var nextEntityId = 1L;
            var timer = new Timer();
            var grid = new MockGridTerminalSystem();

            var programmableBlock = new MockProgrammableBlock()
            {
                CustomName = "Programmable Block [SSM]"
            };

            grid.Add(new MockTextPanel()
            {
                EntityId = nextEntityId++,
                CustomName = "Debug Panel",
                CustomData = "functions:debug lcd"
            });

            var zones = new List<Zone>();
            var airlock = new MockDoor()
            {
                EntityId = nextEntityId++,
                CustomName = "Zone 1/2 Airlock",
                CustomData = "functions:airlock"
            };

            airlock.OpenDoor();

            {
                var room = new Zone("zone-1");
                room.AddBlock(airlock);

                room.AddBlock(new MockAirVent()
                {
                    EntityId = nextEntityId++,
                    Status = VentStatus.Pressurized,
                    CustomName = "Air Vent (Zone 1)",
                    ShowInTerminal = false,
                    IsDepressurizing = false,
                    CanPressurize = true
                });

                room.AddBlock(new MockInteriorLight()
                {
                    EntityId = nextEntityId++,
                    CustomName = "Alert Light (Zone 1)",
                    ShowInTerminal = false,
                    CustomData = "functions:warnlight"
                });

                room.AddBlock(new MockInteriorLight()
                {
                    EntityId = nextEntityId++,
                    CustomName = "Normal Light (Zone 1)",
                    ShowInTerminal = false,
                });

                room.AddBlock(new MockTextPanel()
                {
                    EntityId = nextEntityId++,
                    CustomName = "Door Sign (Zone 1)",
                    CustomData = "functions:doorsign"
                });

                room.OfType<IMyTextPanel>().First(b => b.CustomName == "Door Sign (Zone 1)")
                    .WritePublicText("ZONE 1");

                zones.Add(room);

                room.AddToGrid(grid);
            }

            {
                var room = new Zone("zone-2");
                room.AddBlock(airlock);

                room.AddBlock(new MockAirVent()
                {
                    EntityId = nextEntityId++,
                    Status = VentStatus.Pressurized,
                    CustomName = "Air Vent (Zone 2)",
                    ShowInTerminal = false,
                    IsDepressurizing = false,
                    CanPressurize = true
                });

                room.AddBlock(new MockInteriorLight()
                {
                    EntityId = nextEntityId++,
                    CustomName = "Alert Light (Zone 2)",
                    ShowInTerminal = false,
                    CustomData = "functions:warnlight"
                });

                room.AddBlock(new MockInteriorLight()
                {
                    EntityId = nextEntityId++,
                    CustomName = "Normal Light (Zone 2)",
                    ShowInTerminal = false,
                });

                room.AddBlock(new MockTextPanel()
                {
                    EntityId = nextEntityId++,
                    CustomName = "Door Sign (Zone 2)",
                    CustomData = "functions:doorsign"
                });

                room.OfType<IMyTextPanel>().First(b => b.CustomName == "Door Sign (Zone 2)")
                    .WritePublicText("ZONE 2");

                zones.Add(room);

                room.AddToGrid(grid);
            }

            var program = MDKFactory.CreateProgram<Program>(new MDKFactory.ProgramConfig()
            {
                Echo = message => Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("U"), message),
                GridTerminalSystem = grid,
                ProgrammableBlock = programmableBlock
            });

            Console.WriteLine("Executing Run #1 (Normal)");
            MDKFactory.Run(program, updateType: UpdateType.Update10);
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");
            
            foreach (var light in grid.GetBlocksOfType<IMyInteriorLight>())
            {
                Console.WriteLine($"Testing Light {light.EntityId}.");
                Assert.Equals(light.Color, new Color(255,255,255), $"Light {light.EntityId} does not have the expected color.");
                Assert.Equals(light.BlinkIntervalSeconds, 0, $"Light {light.EntityId} does not have the expected blink interval.");
                Assert.Equals(light.BlinkLength, 0, $"Light {light.EntityId} does not have the expected blink length.");
                Assert.Equals(light.BlinkOffset, 0, $"Light {light.EntityId} does not have the expected blink offset.");
            }
                                
            foreach (var lcd in grid.GetBlocksOfType<IMyTextPanel>())
            {
                Console.WriteLine($"Testing LCD {lcd.EntityId}.");
                switch (lcd.Name)
                {
                    case "Door Sign (Zone 1)":
                        Assert.Equals(lcd.GetPublicText(), "ZONE 1", $"LCD {lcd.EntityId} doesn't have the expected text.");
                        break;
                    case "Door Sign (Zone 2)":
                        Assert.Equals(lcd.GetPublicText(), "ZONE 2", $"LCD {lcd.EntityId} doesn't have the expected text.");
                        break;
                }
            }

            foreach (var door in grid.GetBlocksOfType<IMyDoor>())
            {
                if (door.IsInAllZones("zone-1", "zone-2"))
                {
                    Assert.True(door.Status == DoorStatus.Open, $"Airlock joined to Zone 1 and a 2 should be open.");
                }
            }
            
            foreach (var vent in grid.GetZoneBlocks<MockAirVent>("zone-1"))
            {
                vent.IsDepressurizing = true;
                vent.CanPressurize = false;
            }

            Console.WriteLine("Executing Run #2 (Decompression - Zone 1)");
            MDKFactory.Run(program, updateType: UpdateType.Update10);            
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");
            
            foreach (var light in grid.GetZoneBlocks<MockInteriorLight>("zone-1"))
            {
                Console.WriteLine($"Testing Light {light.EntityId}.");
                if (light.HasFunction(Program.BlockFunction.LIGHT_WARNING))
                {
                    Assert.Equals(light.Color, Styler.Get<Color>("decompression.light.color"), $"Light {light.EntityId} does not have the expected color.");
                    Assert.Equals(light.BlinkIntervalSeconds, Styler.Get<Single>("decompression.light.interval"), $"Light {light.EntityId} does not have the expected blink interval.");
                    Assert.Equals(light.BlinkLength, Styler.Get<Single>("decompression.light.duration"), $"Light {light.EntityId} does not have the expected blink length.");
                    Assert.Equals(light.BlinkOffset, Styler.Get<Single>("decompression.light.offset"), $"Light {light.EntityId} does not have the expected blink offset.");
                    Assert.True(light.Enabled, $"Light {light.EntityId} should be enabled.");
                }
                else
                {
                    Assert.False(light.Enabled, $"Light {light.EntityId} should not be enabled.");
                }
            }

            foreach (var lcd in grid.GetZoneBlocks<IMyTextPanel>("zone-1"))
            {
                Console.WriteLine($"Testing LCD {lcd.EntityId}.");
                switch (lcd.Name)
                {
                    case "Door Sign (Zone 1)":
                        Assert.Equals(lcd.GetPublicText(), Styler.Get<String>("decompression.text"), $"LCD {lcd.EntityId} doesn't have the expected text.");
                        break;
                }
            }

            foreach (var door in grid.GetBlocksOfType<IMyDoor>())
            {
                Console.WriteLine($"Testing door {door.EntityId}.");
                if (door.IsInAllZones("zone-1", "zone-2"))
                {
                    Assert.True(door.Status == DoorStatus.Closed || door.Status == DoorStatus.Closed, $"Airlock joined to Zone 1 and a 2 should be closed or closing.");
                    Assert.True(door.Enabled, $"Airlock joined to Zone 1 and a 2 should be enabled.");
                }
            }

            Console.WriteLine("Executing Run #4 (Decompression - Zone 1)");
            MDKFactory.Run(program, updateType: UpdateType.Update10);
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");

            foreach (var door in grid.GetBlocksOfType<IMyDoor>())
            {
                Console.WriteLine($"Testing door {door.EntityId}.");
                if (door.IsInAllZones("zone-1", "zone-2"))
                {
                    Assert.True(door.Status == DoorStatus.Closed, $"Airlock joined to Zone 1 and a 2 should be closed.");
                    Assert.False(door.Enabled, $"Airlock joined to Zone 1 and a 2 should be disabled.");
                }
            }

            foreach (var vent in grid.GetZoneBlocks<MockAirVent>("zone-1"))
            {
                vent.IsDepressurizing = false;
                vent.CanPressurize = true;
            }

            Console.WriteLine("Executing Run #4 (Normal)");
            MDKFactory.Run(program, updateType: UpdateType.Update10);
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");

            foreach (var light in grid.GetZoneBlocks<MockInteriorLight>("zone-1"))
            {
                Console.WriteLine($"Testing Light {light.EntityId}.");
                Assert.Equals(light.Color, new Color(255, 255, 255), $"Light {light.EntityId} does not have the expected color.");
                Assert.Equals(light.BlinkIntervalSeconds, 0, $"Light {light.EntityId} does not have the expected blink interval.");
                Assert.Equals(light.BlinkLength, 0, $"Light {light.EntityId} does not have the expected blink length.");
                Assert.Equals(light.BlinkOffset, 0, $"Light {light.EntityId} does not have the expected blink offset.");
            }

            var lcds = grid.GetZoneBlocks<IMyTextPanel>("zone-1");

            foreach (var lcd in lcds)
            {
                Console.WriteLine($"Testing LCD {lcd.EntityId}.");
                switch (lcd.Name)
                {
                    case "Door Sign (Zone 1)":
                        Assert.Equals(lcd.GetPublicText(), "ZONE 1", $"LCD {lcd.EntityId} doesn't have the expected text.");
                        break;
                }
            }

            Console.WriteLine("Executing Run #5 (Enable Battle Stations)");
            MDKFactory.Run(program, argument: "activate battle");
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");

            Assert.Equals(programmableBlock.GetConfig("custom-states"), "battle", "Activating battle state did not have desired effect.");

            Console.WriteLine("Executing Run #6 (Enable Self-Destruct)");
            MDKFactory.Run(program, argument: "activate destruct");
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");

            Assert.Equals(programmableBlock.GetConfig("custom-states"), "battle;destruct", "Activating destruct state did not have desired effect.");

            Console.WriteLine("Executing Run #7 (Toggle Battle Stations)");
            MDKFactory.Run(program, argument: "toggle battle");
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");

            Assert.Equals(programmableBlock.GetConfig("custom-states"), "destruct", "Toggling battle state did not have desired effect.");

            Console.WriteLine("Executing Run #8 (Toggle Self-Destruct)");
            MDKFactory.Run(program, argument: "disable destruct");
            Console.WriteLine($"Execution time: {program.Runtime.LastRunTimeMs}ms.");

            Assert.Equals(programmableBlock.GetConfig("custom-states"), "", "Disabling destruct state did not have desired effect.");

            Console.WriteLine();
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("DEBUG LCD OUTPUT:");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(grid.GetBlocksOfType<IMyTextPanel>(t => t.HasFunction("debug lcd")).FirstOrDefault()?.GetPublicText().Trim() ?? ">> NO LCD FOUND <<");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private class Zone : IReadOnlyList<IMyTerminalBlock>
        {
            public String Name { get; }
            public IEnumerable<String> AdjacentZones => Blocks.SelectMany(b => b.GetZones()).Distinct().Where(z => z != Name);
            public List<IMyTerminalBlock> Blocks { get; } = new List<IMyTerminalBlock>();

            public Int32 Count => ((IReadOnlyList<IMyTerminalBlock>)Blocks).Count;

            public IMyTerminalBlock this[Int32 index] => ((IReadOnlyList<IMyTerminalBlock>)Blocks)[index];

            public Zone(String name)
                : base()
            {
                Name = name;
            }

            public void AddBlock(IMyTerminalBlock block)
            {
                Blocks.Add(block);
                block.SetConfigFlag("zones", Name);
            }

            public void RemoveBlock(IMyTerminalBlock block)
            {
                Blocks.Remove(block);
                block.ClearConfigFlag("zones", Name);
            }

            public void AddBlocks(IEnumerable<IMyTerminalBlock> blocks)
            {
                Blocks.AddRange(blocks);
                foreach (var block in blocks)
                {
                    block.SetConfigFlag("zones", Name);
                }
            }

            public void AddToGrid(MockGridTerminalSystem grid)
            {
                var existing = new List<IMyTerminalBlock>();
                grid.GetBlocks(existing);

                foreach (var block in Blocks.Where(b => !existing.Contains(b)))
                {
                    grid.Add(block);
                }
            }

            public IEnumerator<IMyTerminalBlock> GetEnumerator() => ((IReadOnlyList<IMyTerminalBlock>)Blocks).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<IMyTerminalBlock>)Blocks).GetEnumerator();
        }
    }
}