using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tEvasion : PassiveTrait
    {
        const string ID = "evasion";

        public tEvasion() : base(ID)
        {
            name = Translator.GetString("trait_evasion_1");
            desc = Translator.GetString("trait_evasion_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tEvasion(tEvasion other) : base(other) { }
        public override object Clone() => new tEvasion(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_evasion_3");
        }
        //public override BattleWeight Weight(IBattleTrait trait)
        //{
        //    return new(result.Entity, 0, (float)(1 + (Math.E * Math.Log(Math.Pow(trait.GetStacks(), 2) - 1) / 10)));
        //}
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnOwnerInitiationPreReceived);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;

            await trait.AnimActivation();
            e.handled = true;
            await trait.AdjustStacks(-1, trait);
        }
    }
}
