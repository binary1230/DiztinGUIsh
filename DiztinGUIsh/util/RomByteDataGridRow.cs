﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Diz.Core.model;
using Diz.Core.model.byteSources;
using Diz.Core.model.snes;
using Diz.Core.util;
using DiztinGUIsh.controller;
using JetBrains.Annotations;
using Label = Diz.Core.model.Label;

namespace DiztinGUIsh.util
{
    public interface IGridRow<TItem>
    {
        IBytesGridViewer<TItem> ParentView { get; init; }
        Data Data { get; init; }
        ByteEntry ByteEntry { get; init; }
    }
    
    /*[AttributeUsage(AttributeTargets.Property)]
    public class CellStyleFormatter : Attribute
    {
        public Func<Color?> BackgroundColorFormatter { get; }

        public CellStyleFormatter(Func<Color?> bgColorFormatter)
        {
            BackgroundColorFormatter = bgColorFormatter;
        }
    }*/

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class RomByteDataGridRow : INotifyPropertyChanged, IGridRow<ByteEntry>
    {
        [DisplayName("Label")]
        [Editable(true)]
        // [CellStyleFormatter(GetBackColorInOut)]
        public string Label
        {
            get => Data.Labels.GetLabelName(Data.ConvertPCtoSnes(ByteEntry.ParentByteSourceIndex));

            // todo (validate for valid label characters)
            // (note: validation implemented in Furious's branch, integrate here)
            set => Data.Labels.AddLabel(
                    Data.ConvertPCtoSnes(ByteEntry.ParentByteSourceIndex),
                    new Label {Name = value},
                    true);
        }

        [DisplayName("PC")]
        [ReadOnly(true)]
        public string Offset => Util.ToHexString6(Data.ConvertPCtoSnes(ByteEntry.ParentByteSourceIndex));

        // show the byte two different ways: ascii and numeric
        [DisplayName("@")]
        [ReadOnly(true)]
        public char AsciiCharRep =>
            (char) ByteEntry.Byte;

        [DisplayName("#")]
        [ReadOnly(true)]
        public string NumericRep =>
            Util.NumberToBaseString(ByteEntry.ParentByteSourceIndex, NumberBase);

        [DisplayName("<*>")]
        [ReadOnly(true)]
        public string Point =>
            RomUtil.PointToString(ByteEntry.Point);

        [DisplayName("Instruction")]
        [ReadOnly(true)]
        public string Instruction
        {
            get
            {
                // NOTE: this does not handle instructions whose opcodes cross banks correctly.
                // if we hit this situation, just return empty for the grid, it's likely real instruction won't do this?
                var romOffset = ByteEntry.ParentByteSourceIndex;
                var len = Data.GetInstructionLength(romOffset);
                return romOffset + len <= Data.GetRomSize() ? Data.GetInstruction(romOffset) : "";
            }
        }

        [DisplayName("IA")]
        [ReadOnly(true)]
        // ReSharper disable once InconsistentNaming
        public string IA
        {
            get
            {
                var ia = Data.GetIntermediateAddressOrPointer(ByteEntry.ParentByteSourceIndex);
                return ia >= 0 ? Util.ToHexString6(ia) : "";
            }
        }

        [DisplayName("Flag")]
        [ReadOnly(true)]
        public string TypeFlag =>
            Util.GetEnumDescription(Data.GetFlag(ByteEntry.ParentByteSourceIndex));

        [DisplayName("B")]
        [Editable(true)]
        public string DataBank
        {
            get => Util.NumberToBaseString(Data.GetDataBank(ByteEntry.ParentByteSourceIndex), Util.NumberBase.Hexadecimal, 2);
            set
            {
                if (!int.TryParse(value, NumberStyles.HexNumber, null, out var parsed))
                    return;

                Data.SetDataBank(ByteEntry.ParentByteSourceIndex, parsed);
                //OnPropertyChanged();
            }
        }

        [DisplayName("D")]
        [Editable(true)]
        public string DirectPage
        {
            get => Util.NumberToBaseString(Data.GetDirectPage(ByteEntry.ParentByteSourceIndex), Util.NumberBase.Hexadecimal, 4);
            set
            {
                if (!int.TryParse(value, NumberStyles.HexNumber, null, out var parsed))
                    return;

                Data.SetDirectPage(ByteEntry.ParentByteSourceIndex, parsed);
                //OnPropertyChanged();
            }
        }

        [DisplayName("M")]
        [Editable(true)]
        public string MFlag
        {
            get => RomUtil.BoolToSize(Data.GetMFlag(ByteEntry.ParentByteSourceIndex));
            set
            {
                Data.SetMFlag(ByteEntry.ParentByteSourceIndex, value == "8" || value == "M");
                //OnPropertyChanged();
            }
        }

        [DisplayName("X")]
        [Editable(true)]
        public string XFlag
        {
            get => RomUtil.BoolToSize(Data.GetXFlag(ByteEntry.ParentByteSourceIndex));
            set
            {
                Data.SetXFlag(ByteEntry.ParentByteSourceIndex, value == "8" || value == "X");
                // OnPropertyChanged();
            }
        }

        [DisplayName("Comment")]
        [Editable(true)]
        public string Comment
        {
            get => Data.GetCommentText(Data.ConvertPCtoSnes(ByteEntry.ParentByteSourceIndex));
            set
            {
                Data.AddComment(Data.ConvertPCtoSnes(ByteEntry.ParentByteSourceIndex), value, true);
                // OnPropertyChanged();
            }
        }
        
        private readonly ByteEntry byteEntry;

        [Browsable(false)]
        public ByteEntry ByteEntry
        {
            get => byteEntry;
            init
            {
                this.SetField(PropertyChanged, ref byteEntry, value);
                // tmp disable // if (ByteOffset != null)
                // ByteOffset.PropertyChanged += OnRomBytePropertyChanged;
            }
        }

        [Browsable(false)] public Data Data { get; init; }
        [Browsable(false)] public IBytesGridViewer<ByteEntry> ParentView { get; init; }
        [Browsable(false)] private Util.NumberBase NumberBase => ParentView.NumberBaseToShow;

        [Browsable(false)] public event PropertyChangedEventHandler PropertyChanged;

        private void OnRomBytePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            void OnInstructionRelatedChanged()
            {
                OnPropertyChanged(nameof(Instruction));
                OnPropertyChanged(nameof(IA));
            }

            // NOTE: if any properties under ByteOffset change, make sure the names update here
            switch (e.PropertyName)
            {
                case nameof(Diz.Core.model.byteSources.ByteEntry.Byte):
                    OnPropertyChanged(nameof(AsciiCharRep));
                    OnPropertyChanged(nameof(NumericRep));
                    OnInstructionRelatedChanged();
                    break;
                case nameof(Diz.Core.model.byteSources.ByteEntry.Arch):
                    OnInstructionRelatedChanged();
                    break;
                case nameof(Diz.Core.model.byteSources.ByteEntry.DataBank):
                case nameof(Diz.Core.model.byteSources.ByteEntry.DirectPage):
                case nameof(Diz.Core.model.byteSources.ByteEntry.XFlag):
                case nameof(Diz.Core.model.byteSources.ByteEntry.MFlag):
                case nameof(Diz.Core.model.byteSources.ByteEntry.TypeFlag):
                case nameof(Diz.Core.model.byteSources.ByteEntry.Point):
                    OnPropertyChanged(e.PropertyName);
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool IsColumnEditable(string propertyName)
        {
            return TestAttribute((EditableAttribute attr) => attr?.AllowEdit ?? false, propertyName);
        }

        public static string GetColumnDisplayName(string propertyName)
        {
            return TestAttribute((DisplayNameAttribute attr) => attr?.DisplayName, propertyName);
        }
        
        public static bool GetColumnIsReadOnly(string propertyName)
        {
            return TestAttribute((ReadOnlyAttribute attr) => attr?.IsReadOnly ?? false, propertyName);
        }
        
        public static bool IsPropertyBrowsable(string propertyName)
        {
            return TestAttribute((BrowsableAttribute attr) => attr?.Browsable ?? true, propertyName);
        }

        private static TResult TestAttribute<TAttribute, TResult>(
            Func<TAttribute, TResult> getValueFn, string memberName)
            where TAttribute : Attribute
        {
            return Util.GetPropertyAttribute(getValueFn, typeof(RomByteDataGridRow), memberName);
        }

        /// <summary>
        /// Format an arbitrary cell in the grid. it may or may not be the currently selected cell.
        /// </summary>
        /// <param name="colPropName">the name of the data property associated with this column (not the column header, this is the internal name)</param>
        /// <param name="style">Out param, modify this to set the style</param>
        public void SetStyleForCell(string colPropName, DataGridViewCellStyle style)
        {
            if (IsColumnEditable(colPropName))
                style.SelectionBackColor = Color.Chartreuse;

            // all cells in a row get this treatment
            switch (ByteEntry.TypeFlag)
            {
                case FlagType.Unreached:
                    style.BackColor = Color.LightGray;
                    style.ForeColor = Color.DarkSlateGray;
                    break;
                case FlagType.Opcode:
                    var color = GetBackgroundColorForMarkedAsOpcode(colPropName);
                    if (color != null)
                        style.BackColor = color.Value;
                    break;
                case FlagType.Operand:
                    style.ForeColor = Color.LightGray;
                    break;
                case FlagType.Graphics:
                    style.BackColor = Color.LightPink;
                    break;
                case FlagType.Music:
                    style.BackColor = Color.PowderBlue;
                    break;
                case FlagType.Data8Bit:
                case FlagType.Data16Bit:
                case FlagType.Data24Bit:
                case FlagType.Data32Bit:
                    style.BackColor = Color.NavajoWhite;
                    break;
                case FlagType.Pointer16Bit:
                case FlagType.Pointer24Bit:
                case FlagType.Pointer32Bit:
                    style.BackColor = Color.Orchid;
                    break;
                case FlagType.Text:
                    style.BackColor = Color.Aquamarine;
                    break;
                case FlagType.Empty:
                    style.BackColor = Color.DarkSlateGray;
                    style.ForeColor = Color.LightGray;
                    break;
            }

            SetStyleForIndirectAddress(colPropName, style);
        }

        private Color? GetBackgroundColorForMarkedAsOpcode(string colPropName)
        {
            // TODO: eventually, don't match strings here.
            // instead, look for the appropriate attribute attached to romByteRow and let that 
            // attribute hook in here.
            return colPropName switch
            {
                nameof(Point) => GetBackColorInOut(),
                nameof(Instruction) => GetInstructionBackgroundColor(),
                nameof(DataBank) => GetDataBankColor(),
                nameof(DirectPage) => GetDirectPageColor(),
                nameof(MFlag) => GetMFlagColor(),
                nameof(XFlag) => GetXFlagColor(),
                _ => null
            };
        }

        private Color? GetBackColorInOut()
        {
            int r = 255, g = 255, b = 255;
            if ((ByteEntry.Point & (InOutPoint.EndPoint | InOutPoint.OutPoint)) != 0) g -= 50;
            if ((ByteEntry.Point & InOutPoint.InPoint) != 0) r -= 50;
            if ((ByteEntry.Point & InOutPoint.ReadPoint) != 0) b -= 50;
            return Color.FromArgb(r, g, b);
        }

        private Color? GetInstructionBackgroundColor()
        {
            var opcode = ByteEntry.Byte;
            var isWeirdInstruction =
                    opcode == 0x40 || opcode == 0xCB || opcode == 0xDB || opcode == 0xF8 || // RTI WAI STP SED
                    opcode == 0xFB || opcode == 0x00 || opcode == 0x02 || opcode == 0x42 // XCE BRK COP WDM
                ;
            return isWeirdInstruction ? Color.Yellow : null;
        }

        private Color? GetDataBankColor()
        {
            switch (ByteEntry.Byte)
            {
                // PLB MVP MVN
                case 0xAB:
                case 0x44:
                case 0x54:
                    return Color.OrangeRed;
                // PHB
                case 0x8B:
                    return Color.Yellow;
                default:
                    return null;
            }
        }

        private Color? GetDirectPageColor()
        {
            switch (ByteEntry.Byte)
            {
                // PLD TCD
                case 0x2B:
                case 0x5B:
                    return Color.OrangeRed;

                // PHD TDC
                case 0x0B:
                case 0x7B:
                    return Color.Yellow;

                default:
                    return null;
            }
        }

        public Color? GetMFlagColor() => GetMxFlagColor(0x20);
        public Color? GetXFlagColor() => GetMxFlagColor(0x10);

        private Color? GetMxFlagColor(int nextByteMask)
        {
            var nextByte = Data.GetNextRomByte(ByteEntry.ParentByteSourceIndex) ?? 0;
            switch (ByteEntry.Byte)
            {
                // PLP
                // SEP REP, *iff* relevant bit is set on next byte
                case 0x28:
                case 0xC2 or 0xE2 when (nextByte & nextByteMask) != 0:
                    return Color.OrangeRed;
                case 0x08: // PHP
                    return Color.Yellow;
                default:
                    return null;
            }
        }

        private void SetStyleForIndirectAddress(string colPropName, DataGridViewCellStyle style)
        {
            var selectedRomByteRow = ParentView.SelectedByteOffset;
            if (selectedRomByteRow == null)
                return;

            var matchingIa = colPropName switch
            {
                nameof(Offset) => 
                    Data.IsMatchingIntermediateAddress(selectedRomByteRow.ParentByteSourceIndex, ByteEntry.ParentByteSourceIndex),
                nameof(IA) => 
                    Data.IsMatchingIntermediateAddress(ByteEntry.ParentByteSourceIndex, selectedRomByteRow.ParentByteSourceIndex),
                _ => false
            };

            if (matchingIa)
                style.BackColor = Color.DeepPink;
        }
    }
    
    // TODO: consider moving all of this into some per-property attribute would be something like?
    // [CustomConfig(col =>
    // {
    //     col.DefaultCellStyle = new DataGridViewCellStyle
    //     {
    //         Alignment = DataGridViewContentAlignment.MiddleRight, Font = FontHuman,
    //     };
    //     col.MaxInputLength = 60;
    //     col.MinimumWidth = 6;
    //     col.Width = 200;
    // })]
    public static class RomByteDataGridRowFormatting {
        public static readonly Font FontData = new("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static readonly Font FontHuman = new("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

        private static readonly Dictionary<string, Action<DataGridViewTextBoxColumn>> CellProperties = new()
        {
            {
                nameof(RomByteDataGridRow.Label), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight, Font = FontHuman,
                    };
                    col.MaxInputLength = 60;
                    col.MinimumWidth = 6;
                    col.Width = 200;
                }
            },
            {
                nameof(RomByteDataGridRow.Offset), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft, Font = FontData,
                    };
                    col.MaxInputLength = 6;
                    col.MinimumWidth = 6;
                    col.Width = 58;
                }
            },
            {
                nameof(RomByteDataGridRow.AsciiCharRep), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight, Font = FontData,
                    };
                    col.MaxInputLength = 1;
                    col.MinimumWidth = 6;
                    col.Width = 26;
                }
            },
            {
                nameof(RomByteDataGridRow.NumericRep), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight, Font = FontData,
                    };
                    col.MaxInputLength = 3;
                    col.MinimumWidth = 6;
                    col.Width = 26;
                }
            },
            {
                nameof(RomByteDataGridRow.Point), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter, Font = FontData,
                    };
                    col.MaxInputLength = 3;
                    col.MinimumWidth = 6;
                    col.Width = 34;
                }
            },
            {
                nameof(RomByteDataGridRow.Instruction), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft, Font = FontData,
                    };
                    col.MaxInputLength = 64;
                    col.MinimumWidth = 6;
                    col.Width = 125;
                }
            },
            {
                nameof(RomByteDataGridRow.IA), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft, Font = FontData,
                    };
                    col.MaxInputLength = 6;
                    col.MinimumWidth = 6;
                    col.Width = 58;
                }
            },
            {
                nameof(RomByteDataGridRow.TypeFlag), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft, Font = FontData,
                    };
                    col.MinimumWidth = 6;
                    col.Width = 86;
                }
            },
            {
                nameof(RomByteDataGridRow.DataBank), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleRight, Font = FontData,
                    };
                    col.MaxInputLength = 2;
                    col.MinimumWidth = 6;
                    col.Width = 26;
                }
            },
            {
                nameof(RomByteDataGridRow.DirectPage), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft, Font = FontData,
                    };
                    col.MaxInputLength = 4;
                    col.MinimumWidth = 6;
                    col.Width = 42;
                }
            },
            {
                nameof(RomByteDataGridRow.MFlag), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter, Font = FontData,
                    };
                    col.MaxInputLength = 2;
                    col.MinimumWidth = 6;
                    col.Width = 26;
                }
            },
            {
                nameof(RomByteDataGridRow.XFlag), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleCenter, Font = FontData,
                    };
                    col.MaxInputLength = 2;
                    col.MinimumWidth = 6;
                    col.Width = 26;
                }
            },
            {
                nameof(RomByteDataGridRow.Comment), col =>
                {
                    col.DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Alignment = DataGridViewContentAlignment.MiddleLeft, Font = FontHuman,
                        // WrapMode = DataGridViewTriState.False, // TODO: consider this?
                    };
                    col.MinimumWidth = 6;
                    // col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // ULTRA EXPENSIVE, never use.
                }
            },
        };

        public static void ApplyFormatting(DataGridViewTextBoxColumn col) => CellProperties[col.DataPropertyName](col);
    }
}