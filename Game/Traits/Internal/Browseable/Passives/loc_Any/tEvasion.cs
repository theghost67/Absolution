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
    public class tEvasion : PassiveTrait
    {
        const string ID = "evasion";
        const int PRIORITY = 1;

        public tEvasion() : base(ID)
        {
            name = "Уклонение";
            desc = "И что ты сделаешь? Что ты сделаешь?";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tEvasion(tEvasion other) : base(other) { }
        public override object Clone() => new tEvasion(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Перед атакой на владельца (П{PRIORITY})</color>\nОтменяет данную атаку на владельца. Тратит один заряд.";
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
            await trait.AdjustStacks(-1, trait);
            e.handled = true;
        }
    }
}
