using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
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

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"Перед получением атакующей инициации владельцем (П{PRIORITY})",
                    $"Уменьшает количество своих зарядов на 1 и силу инициации до нуля."),
            });
        }
        public override BattleWeight Weight(IBattleTrait trait)
        {
            return new(0, Mathf.Log(4, 3 + trait.GetStacks()));
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 60 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPreReceived.Add(trait.GuidStr, OnOwnerInitiationPreReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPreReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPreReceived(object sender, BattleInitiationRecvArgs rArgs)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await rArgs.strength.SetValue(0, trait);
        }
    }
}
