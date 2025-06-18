using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tBrawl : ActiveTrait
    {
        const string ID = "brawl";
        const int CD = 2;

        public tBrawl() : base(ID)
        {
            name = Translator.GetString("trait_brawl_1");
            desc = Translator.GetString("trait_brawl_2");

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerDouble);
        }
        protected tBrawl(tBrawl other) : base(other) { }
        public override object Clone() => new tBrawl(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_brawl_3", CD);

        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return new(result.Entity, result.Field.Card.Strength);
        }

        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle && e.target.Card != null;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard target = (BattleFieldCard)e.target.Card;
            BattleFieldCard owner = trait.Owner;

            if (owner.Drawer != null)
            {
                int yVector = owner.Side.isMe ? -1 : 1;
                int xVector = owner.Field.pos.x > target.Field.pos.x ? -1 : 1;
                Vector3 subVector = 30 * new Vector3(xVector, yVector);
                Vector3 endPos = target.Drawer.transform.position + subVector;
                Vector3 endEuler = Vector3.back * (xVector * 30);
                Tween lungeRotTween = target.Drawer.transform.DORotate(endEuler, 0.66f).SetEase(Ease.InOutCubic);
                Tween lungePosTween = target.Drawer.transform.DOMove(endPos, 0.66f).SetEase(Ease.InOutCubic);
                await lungePosTween.AsyncWaitForCompletion();
                endPos = owner.Field.Opposite.Drawer.transform.position + subVector;
                endEuler = Vector3.zero;
                Tween throwRotTween = target.Drawer.transform.DORotate(endEuler, 0.15f);
                Tween throwPosTween = target.Drawer.transform.DOMove(endPos, 0.15f);
                await throwPosTween.AsyncWaitForCompletion();
            }

            BattleField opposite = owner.Field.Opposite;
            if (opposite.Card != null)
            {
                int damage = target.Strength;
                await e.AnimActivationShort($"{name}!\n-{damage}");
                await opposite.Card.Health.AdjustValue(-damage, target);
            }

            if (opposite.Card == null || opposite.Card.IsKilled)
                await target.TryAttachToField(opposite, target);
            if (target.Field != opposite)
                target.Field.Drawer?.AnimAttachCard(target.Drawer);

            trait.SetCooldown(CD);
        }
    }
}
