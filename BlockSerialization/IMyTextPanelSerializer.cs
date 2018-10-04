﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Serialization
    {
        public class IMyTextPanelSerializer : IMyFunctionalBlockSerializer<IMyTextPanel>
        {
            protected override void Serialize(IMyTextPanel block, Dictionary<String, Object> values)
            {
                base.Serialize(block, values);

                values.Add(nameof(block.BackgroundColor), block.BackgroundColor.PackedValue);
                values.Add(nameof(block.ChangeInterval), block.ChangeInterval);
                values.Add(nameof(block.Font), block.Font);
                values.Add(nameof(block.FontColor), block.FontColor.PackedValue);
                values.Add(nameof(block.FontSize), block.FontSize);
                values.Add(nameof(block.ShowText), block.ShowText);
                values.Add(nameof(CustomProperties.PublicText), block.GetPublicText().Replace("\n", "#NL#"));
                values.Add(nameof(CustomProperties.PublicTitle), block.GetPublicTitle());

                var images = new List<String>();
                block.GetSelectedImages(images);
                values.Add(nameof(CustomProperties.Images), String.Join(";", images));
            }

            protected override void Deserialize(IMyTextPanel block, Dictionary<String, Object> values)
            {
                base.Deserialize(block, values);

                foreach (var value in values)
                {
                    switch (value.Key)
                    {
                        case nameof(block.BackgroundColor):
                            block.BackgroundColor = new VRageMath.Color(Convert.ToInt64(value.Value)); break;
                        case nameof(block.ChangeInterval):
                            block.ChangeInterval = Convert.ToSingle(value.Value); break;
                        case nameof(block.Font):
                            var font = Convert.ToString(value.Value);
                            var fonts = new List<String>();
                            block.GetFonts(fonts);
                            if (fonts.Contains(font))
                                block.Font = font;
                            break;
                        case nameof(block.FontColor):
                            block.FontColor = new VRageMath.Color(Convert.ToInt64(value.Value)); break;
                        case nameof(block.FontSize):
                            block.FontSize = Convert.ToSingle(value.Value); break;
                        case nameof(block.ShowText):
                            var show = Convert.ToBoolean(value.Value);
                            if (show)
                            {
                                block.ShowPublicTextOnScreen();
                            }
                            else
                            {
                                block.ShowTextureOnScreen();
                            }
                            break;
                        case nameof(CustomProperties.PublicText):
                            block.WritePublicText(value.Value.ToString().Replace("#NL#", "\n"), false);
                            break;
                        case nameof(CustomProperties.PublicTitle):
                            block.WritePublicTitle(value.Value.ToString(), false);
                            break;
                        case nameof(CustomProperties.Images):
                            var images = value.Value.ToString().Split(';').ToList();
                            block.ClearImagesFromSelection();
                            block.AddImagesToSelection(images, true);
                            break;
                    }
                }
            }
        }
    }
}