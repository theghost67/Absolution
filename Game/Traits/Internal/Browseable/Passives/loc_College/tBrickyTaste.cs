using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBrickyTaste : PassiveTrait
    {
        const string ID = "bricky_taste";
        const int PRIORITY = 2;
        const int MOXIE_DECREASE = 1;

        public tBrickyTaste() : base(ID)
        {
            name = "Кирпичный привкус";
            desc = "Ты сейчас кирпич зубами грызть будешь.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tBrickyTaste(tBrickyTaste other) : base(other) { }
        public override object Clone() => new tBrickyTaste(this);

        public override string DescRich(ITableTrait trait)
        {
            int effect = MOXIE_DECREASE * trait.GetStacks();
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После получения любой инициации владельцем (П{PRIORITY})",
                    $"уменьшает инициативу инициатора на <u>{effect}</u> ед."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + 12 * Mathf.Pow(stacks - 1, 2);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            BattlePassiveTrait trait = (BattlePassiveTrait)e.Trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationPostReceived.Add(trait.GuidStr, OnOwnerInitiationPostReceived, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationPostReceived.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationPostReceived(object sender, BattleInitiationRecvArgs rArgs)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            BattlePassiveTrait trait = owner.Traits.Passive(ID);
            if (trait == null) return;

            await trait.AnimActivation();
            await rArgs.Sender.moxie.AdjustValue(-MOXIE_DECREASE * trait.GetStacks(), trait);
        }
    }
}
