using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBlock : PassiveTrait
    {
        const string ID = "block";
        const int PRIORITY = 1;

        public tBlock() : base(ID)
        {
            name = "Блок";
            desc = "Что, думал я просто буду стоять?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tBlock(tBlock other) : base(other) { }
        public override object Clone() => new tBlock(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед атакой на владельца (П{PRIORITY})</color>\nУменьшает количество своих зарядов на 1 и силу атаки до нуля.";
        }
        public override BattleWeight Weight(IBattleTrait trait)
        {
            return new(0, (float)(1 + (Math.E * Math.Log(Math.Pow(trait.GetStacks(), 2) - 1) / 10)));
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnOwnerInitiationPreReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await e.Strength.SetValue(0, trait);
        }
    }
}
