﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    public class MyConfig : IDisposable
    {
        private String Section => "Ship Systems Manager";
        private IMyTerminalBlock Block { get; }
        private MyIni Config { get; } = new MyIni();
        private Boolean Changed { get; set; } = false;

        public MyConfig(IMyTerminalBlock block)
        {
            Block = block;
            Config.TryParse(block.CustomData);
        }

        public void Dispose()
        {
            if (Changed)
            {
                Block.CustomData = Config.ToString();
            }
        }

        public void Clear() => Config.Clear();

        public Boolean ContainsKey(String key) => Config.ContainsKey(Section, key);

        public MyIniValue GetValue(String key) => Config.Get(Section, key);

        public IEnumerable<String> GetValues(String key) => GetValue(key).ToString().Split('\n');

        public void ClearValue(String key) => Config.Set(Section, key, null);

        public void SetValue(String key, Object value)
        {
            Changed = true;
            if (value == null)
            {
                Config.Set(Section, key, null);
                return;
            }

            Config.Set(Section, key, value);
        }

        public void SetValues(String key, IEnumerable<String> values) => SetValue(key, String.Join("\n", values));
        public void SetValues(String key, params String[] values) => SetValues(key, values.AsEnumerable());

        public void AddValue(String key, String value)
        {
            var values = GetValues(key).ToList();
            if (!values.Contains(value))
            {
                values.Add(value);
                SetValues(key, values);
            }
        }

        public void ClearValue(String key, String value)
        {
            var values = GetValues(key).ToList();
            if (values.Contains(value))
            {
                values.Remove(value);
                SetValues(key, values);
            }
        }
    }
}