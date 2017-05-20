﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.ThemeEditor;

namespace Filtration.ObjectModel
{
    public interface IItemFilterScript
    {
        ObservableCollection<IItemFilterBlockBase> ItemFilterBlocks { get; }
        ObservableCollection<ItemFilterBlockGroup> ItemFilterBlockGroups { get; }
        ThemeComponentCollection ThemeComponents { get; set; }
        string FilePath { get; set; }
        string Description { get; set; }
        DateTime DateModified { get; set; }
        IItemFilterScriptSettings ItemFilterScriptSettings { get; }

        List<string> Validate();
        void ReplaceColors(ReplaceColorsParameterSet replaceColorsParameterSet);
    }

    public class ItemFilterScript : IItemFilterScript
    {
        public ItemFilterScript()
        {
            ItemFilterBlocks = new ObservableCollection<IItemFilterBlockBase>();
            ItemFilterBlockGroups = new ObservableCollection<ItemFilterBlockGroup>
            {
                new ItemFilterBlockGroup("Root", null)
            };
            ThemeComponents = new ThemeComponentCollection { IsMasterCollection = true};
            ItemFilterScriptSettings = new ItemFilterScriptSettings(ThemeComponents);
        }

        public ObservableCollection<IItemFilterBlockBase> ItemFilterBlocks { get; }
        public ObservableCollection<ItemFilterBlockGroup> ItemFilterBlockGroups { get; }

        public ThemeComponentCollection ThemeComponents { get; set; } 

        public IItemFilterScriptSettings ItemFilterScriptSettings { get; }

        public string FilePath { get; set; }
        public string Description { get; set; }
        public DateTime DateModified { get; set; }

        public List<string> Validate()
        {
            var validationErrors = new List<string>();

            if (ItemFilterBlocks.Count == 0)
            {
                validationErrors.Add("A script must have at least one block");
            }

            return validationErrors;
        }
        
        public void ReplaceColors(ReplaceColorsParameterSet replaceColorsParameterSet)
        {
            foreach (var block in ItemFilterBlocks.OfType<ItemFilterBlock>().Where(b => BlockIsColorReplacementCandidate(replaceColorsParameterSet, b)))
            {
                if (replaceColorsParameterSet.ReplaceTextColor)
                {
                    var textColorBlockItem = block.BlockItems.OfType<TextColorBlockItem>().First();
                    textColorBlockItem.Color = replaceColorsParameterSet.NewTextColor;
                }
                if (replaceColorsParameterSet.ReplaceBackgroundColor)
                {
                    var backgroundColorBlockItem = block.BlockItems.OfType<BackgroundColorBlockItem>().First();
                    backgroundColorBlockItem.Color = replaceColorsParameterSet.NewBackgroundColor;
                }
                if (replaceColorsParameterSet.ReplaceBorderColor)
                {
                    var borderColorBlockItem = block.BlockItems.OfType<BorderColorBlockItem>().First();
                    borderColorBlockItem.Color = replaceColorsParameterSet.NewBorderColor;
                }
            }
        }

        private bool BlockIsColorReplacementCandidate(ReplaceColorsParameterSet replaceColorsParameterSet, IItemFilterBlock block)
        {
            var textColorItem = block.HasBlockItemOfType<TextColorBlockItem>()
                ? block.BlockItems.OfType<TextColorBlockItem>().First()
                : null;
            var backgroundColorItem = block.HasBlockItemOfType<BackgroundColorBlockItem>()
                ? block.BlockItems.OfType<BackgroundColorBlockItem>().First()
                : null;
            var borderColorItem = block.HasBlockItemOfType<BorderColorBlockItem>()
                ? block.BlockItems.OfType<BorderColorBlockItem>().First()
                : null;

            // If we don't have all of the things we want to replace, then we aren't a candidate for replacing those things.
            if ((textColorItem == null && replaceColorsParameterSet.ReplaceTextColor) ||
                (backgroundColorItem == null && replaceColorsParameterSet.ReplaceBackgroundColor) ||
                (borderColorItem == null && replaceColorsParameterSet.ReplaceBorderColor))
            {
                return false;
            }

            if ((replaceColorsParameterSet.ReplaceTextColor &&
                 textColorItem.Color != replaceColorsParameterSet.OldTextColor) ||
                (replaceColorsParameterSet.ReplaceBackgroundColor &&
                 backgroundColorItem.Color != replaceColorsParameterSet.OldBackgroundColor) ||
                (replaceColorsParameterSet.ReplaceBorderColor &&
                 borderColorItem.Color != replaceColorsParameterSet.OldBorderColor))
            {
                return false;
            }

            return true;
        }

    }
}
