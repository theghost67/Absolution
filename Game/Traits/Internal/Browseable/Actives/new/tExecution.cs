using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tExecution : ActiveTrait
    {
        const string ID = "execution";
        const string TRAIT_ID = "bad_karma";
        const int TRAIT_STACKS = 3;
        static readonly TraitStatFormula _strengthF = new(false, 9999, 0);
        static readonly TraitStatFormula _moxieF = new(false, 5, 0);

        public tExecution() : base(ID)
        {
            name = "<color=#FF00FF>Казнь</color>";
            desc = "Готовься.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeTriple);

            frequency = 0.05f;
        }
        protected tExecution(tExecution other) : base(other) { }
        public override object Clone() => new tExecution(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>При активации на цели с {TRAIT_STACKS} и более зарядами навыка <nobr><u>{traitName}</u></nobr></color>\n" +
                   $"Совершает атаку на цель с силой в {_strengthF.Format(args.stacks, true)}. Если цель была убита после данной атаки, " +
                   $"навсегда удаляет её из колоды стороны-владельца и убивает владельца (игнор. всего). Тратит все заряды.\n\n" +
                   $"<color>Постоянный эффект</color>\nУвеличивает инициативу владельца на {_moxieF.Format(args.stacks)} ед.";
        }
        public override BattleWeight Weight(IBattleTrait trait)
        {
            IEnumerable<BattleFieldCard> oppositeCards = trait.Side.Opposite.Fields().WithCard().Select(f => f.Card);
            IEnumerable<BattleFieldCard> oppositeCardsThatCanBeKilledByTrait = oppositeCards.Where(c => c.Traits.Passive(TRAIT_ID)?.GetStacks() >= TRAIT_STACKS);
            bool someOppositeCardsThatCanBeKilledByTrait = oppositeCardsThatCanBeKilledByTrait.Any();
            if (someOppositeCardsThatCanBeKilledByTrait)
                 return new(trait, oppositeCardsThatCanBeKilledByTrait.Sum(c => c.CalculateWeight(trait.Guid).absolute)); // TODO: replace with "new(1000)" ?
            else return BattleWeight.Zero(trait);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                await trait.Owner.Moxie.AdjustValue(_moxieF.ValueInt(trait.GetStacks()), trait, trait.GuidStr);
            else if (trait.WasRemoved(e))
                await trait.Owner.Moxie.RevertValue(trait.GuidStr);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null && e.target.Card.Traits.Passive(TRAIT_ID)?.GetStacks() >= TRAIT_STACKS;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            BattleFieldCard owner = trait.Owner;
            BattleInitiationSendArgs initiation = new(owner, _strengthF.ValueInt(trait.GetStacks()), true, false, target.Field);
            initiation.OnConfirmed.Add(trait.GuidStr, OnInitiationConfirmed);
            await owner.Territory.Initiations.EnqueueAndAwait(initiation);
            if (!target.IsKilled) return;
            await owner.TryKill(BattleKillMode.IgnoreEverything, trait);
        }

        private async UniTask OnInitiationConfirmed(object sender, BattleInitiationRecvArgs args)
        {
            if (args.ReceiverCard == null || !args.ReceiverCard.IsKilled) return;
            args.ReceiverCard.Drawer.CreateTextAsSpeech(name, Color.red);
            if (!args.ReceiverCard.Side.isMe) return; // works only for player
            Player.Deck.fieldCards.Remove(args.ReceiverCard.Data);
        }
    }
}
