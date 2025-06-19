using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using GreenOne;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMindSplit : ActiveTrait
    {
        const string ID = "mind_split";
        const int CD = 1;
        const string CARD_ID = "maxwell_clone";

        public tMindSplit() : base(ID)
        {
            name = Translator.GetString("trait_mind_split_1");
            desc = Translator.GetString("trait_mind_split_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tMindSplit(tMindSplit other) : base(other) { }
        public override object Clone() => new tMindSplit(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return Translator.GetString("trait_mind_split_3", cardName, CD, cardName, CD);

        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new() { new CardDescriptiveArgs(CARD_ID) };
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, 0, 0.125f);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && 
                ((e.target.Card == null && e.trait.Owner.Health > 1) || (e.target.Card != null && e.target.Card.Data.id == CARD_ID));
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;

            if (target.Card == null)
            {
                int ownerHalfHealth = (int)Mathf.Ceil(owner.Health.ValueAbs / 2f);
                int ownerHalfStrength = (int)Mathf.Ceil(owner.Strength.ValueAbs / 2f);
                await owner.Health.AdjustValue(-ownerHalfHealth, trait);
                await owner.Strength.AdjustValue(-ownerHalfStrength, trait);
                FieldCard clone = CardBrowser.NewField(CARD_ID);
                clone.health = ownerHalfHealth;
                clone.strength = ownerHalfStrength;
                clone.moxie = owner.Moxie;
                clone.price.value = 1;
                await owner.Territory.PlaceFieldCard(clone, target, trait);
            }
            else
            {
                BattleFieldCard clone = target.Card;
                int cloneHealth = clone.Health.ValueAbs.Ceiling();
                int cloneStrength = clone.Strength.ValueAbs.Ceiling();
                await clone.TryKill(BattleKillMode.IgnoreHealthRestore, trait);
                if (!clone.IsKilled)
                {
                    clone.Drawer?.CreateTextAsSpeech(Translator.GetString("trait_mind_split_4"), Color.red);
                    return;
                }
                await owner.Health.AdjustValue(cloneHealth, trait);
                await owner.Strength.AdjustValue(cloneStrength, trait);
            }
        
            trait.SetCooldown(CD);
        }
    }
}
