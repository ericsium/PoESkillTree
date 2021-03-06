using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for crafting unique items.
    /// </summary>
    public class UniqueCraftingViewModel : AbstractCraftingViewModel<UniqueBase>
    {
        public ModSelectorViewModel MsExplicits { get; } = new ModSelectorViewModel(false);

        public UniqueCraftingViewModel(EquipmentData equipmentData)
            : base(equipmentData, equipmentData.UniqueBases)
        {
            MsExplicits.PropertyChanged += MsOnPropertyChanged;

            Init();
        }

        protected override void UpdateBaseSpecific()
        {

            if (SelectedBase.ExplicitMods.Any())
            {
                MsExplicits.Affixes = new[]
                {
                    new Affix(new ItemModTier(SelectedBase.ExplicitMods))
                };
            }
            else
            {
                MsImplicits.Affixes = null;
            }
        }

        protected override IEnumerable<ItemMod> RecalculateItemSpecific()
        {
            Item.NameLine = SelectedBase.UniqueName;
            Item.Frame = FrameType.Unique;
            return MsExplicits.GetExactMods()
                .GroupBy(m => m.Attribute)
                .Select(g => g.Aggregate((m1, m2) => m1.Sum(m2)));
        }

        private void MsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
            }
        }
    }
}