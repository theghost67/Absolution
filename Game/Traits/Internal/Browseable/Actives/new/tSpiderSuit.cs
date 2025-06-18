using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSpiderSuit : ActiveTrait
    {
        const string ID = "spider_suit";
        const string KEY = "spider";
        const string SPIDER_MAN_CARD_ID = "spider_man";
        static readonly TraitStatFormula _strengthF = new(true, 1.00f, 0.00f);
        static readonly TraitStatFormula _moxieF = new(false, 3, 0);

        public tSpiderSuit() : base(ID)
        {
            name = Translator.GetString("trait_spider_suit_1");
            desc = Translator.GetString("trait_spider_suit_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tSpiderSuit(tSpiderSuit other) : base(other) { }
        public override object Clone() => new tSpiderSuit(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            object isInSpiderMode = null;
            args.table?.Storage.TryGetValue(KEY, out isInSpiderMode);
            string str = Translator.GetString("trait_spider_suit_3", _moxieF.Format(args.stacks), _strengthF.Format(args.stacks, true));

            if (isInSpiderMode != null)
            {
                string spiderModeStr = (bool)isInSpiderMode ? Translator.GetString("trait_spider_suit_4") : Translator.GetString("trait_spider_suit_5");
                str += Translator.GetString("trait_spider_suit_6", spiderModeStr);
            }
            return str;
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
			return BattleWeight.One(result.Entity);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            if (e.trait.WasAdded(e))
                e.trait.Storage[KEY] = false;
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleField target = (BattleField)e.target;
            BattleFieldCard owner = trait.Owner;
            
            if ((bool)trait.Storage[KEY] == false)
            {
                trait.Storage[KEY] = true;
                if (owner.Drawer != null && owner.Data.id != SPIDER_MAN_CARD_ID)
                {
                    Sprite sprite = Resources.Load<Sprite>(CardBrowser.GetCard(SPIDER_MAN_CARD_ID).spritePath);
                    owner.Drawer?.RedrawSprite(sprite);
                }

                await owner.Moxie.AdjustValue(_moxieF.ValueInt(e.traitStacks), trait, trait.GuidStr);
                await owner.Strength.AdjustValueScale(-_strengthF.Value(e.traitStacks), trait, trait.GuidStr);
            }
            else
            {
                trait.Storage[KEY] = false;
                if (owner.Drawer != null && owner.Data.id != SPIDER_MAN_CARD_ID)
                {
                    Sprite sprite = Resources.Load<Sprite>(owner.Data.spritePath);
                    owner.Drawer?.RedrawSprite(sprite);
                }

                await owner.Moxie.RevertValue(trait.GuidStr);
                await owner.Strength.RevertValueScale(trait.GuidStr);
            }
        }
    }
}
