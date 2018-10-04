﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    public partial class Program
    {
        public abstract class BaseStyler
        {
            public Int32 Priority { get; protected set; }
            public String State { get; protected set; }

            protected abstract String StylePrefix { get; }
            public static IReadOnlyDictionary<String, Object> DefaultStyles { get; } = new Dictionary<String, Object>()
            {
                { "battle.text", "BATTLE STATIONS" },
                { "battle.text.size", 2.9f },
                { "battle.text.font", "Debug" },
                { "battle.text.color", new Color(255, 0, 0) },
                { "battle.light.color", new Color(255, 0, 0) },
                { "battle.light.interval", 3f },
                { "battle.light.duration", 33.3f },
                { "battle.light.offset", 0f },
                { "battle.sign.color", new Color(0, 0, 0) },
                { "battle.sign.images", "Cross" },
                { "battle.sound", "Alert 1" },

                { "decompression.text", "DECOMPRESSION DANGER" },
                { "decompression.text.size", 2.9f },
                { "decompression.text.font", "Debug" },
                { "decompression.text.color", new Color(0, 0, 255) },
                { "decompression.light.color", new Color(0, 0, 255) },
                { "decompression.light.interval", 3f },
                { "decompression.light.duration", 66.6f },
                { "decompression.light.offset", 0f },
                { "decompression.sign.color", new Color(0, 0, 0) },
                { "decompression.sign.images", "Danger" },
                { "decompression.sound", "Alert 2" },

                { "destruct.text", "SELF DESTRUCT in {0}" },
                { "destruct.sign.images", "Danger" },
                { "destruct.timer", 180f },

                { "intruder.text", "INTRUDER ALERT" },
                { "intruder.text.color", new Color(255, 0, 0) },
                { "intruder.text.size", 2.9f },
                { "intruder.sign.color", new Color(0, 0, 0) },
                { "intruder.sign.images", "Danger" },
                { "intruder.light.color", new Color(255, 0, 0) },
                { "intruder.sound", "Alert 1" },

            };

            protected IMyProgrammableBlock ProgrammableBlock { get; }

            protected BaseStyler(Int32 priority, String key, IMyProgrammableBlock block)
            {
                Priority = priority;
                State = key;
                ProgrammableBlock = block;
            }

            protected T GetStyle<T>(String key)
            {
                key = StylePrefix + "." + key;
                var custom = ProgrammableBlock.GetConfig<T>(key);
                if (ProgrammableBlock.GetConfig().ContainsKey(key))
                {
                    return ProgrammableBlock.GetConfig<T>(key);
                }
                else
                {
                    return (T) DefaultStyles[key];
                }
            }

            public abstract void Style(IMyTerminalBlock block);
        }
    }
}