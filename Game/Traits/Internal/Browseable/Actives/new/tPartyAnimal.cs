using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using GreenOne;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tPartyAnimal : ActiveTrait
    {
        const string ID = "party_animal";
        const string KEY = "animal";
        const string JAKE_CARD_ID = "jake";
        const string JAKE_DANCER_CARD_ID = "jake_dancer";
        Sequence _animSeq;

        public tPartyAnimal() : base(ID)
        {
            name = "Тусовщик";
            desc = "Слушай, ты всё делаешь неверно. Смотри и учись, раз, два, три!";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tPartyAnimal(tPartyAnimal other) : base(other) { }
        public override object Clone() => new tPartyAnimal(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            object isInDanceMode = null;
            args.table?.Storage.TryGetValue(KEY, out isInDanceMode);
            string str = $"<color>При активации на территории</color>\nПереключает режим тусовщика. " +
                         $"Пока находится в режиме тусовщика, владелец игнорирует все внешние попытки наложения навыков.";
            if (isInDanceMode != null)
            {
                string danceModeStr = ((bool)isInDanceMode) == true ? "<color=green>ВКЛ</color>" : "<color=red>ВЫКЛ</color>";
                str += $" Режим тусовщика: {danceModeStr}.";
            }
            return str;
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            if (result.Entity.Storage.ContainsKey(KEY))
			     return BattleWeight.One(result.Entity);
            else return BattleWeight.Zero(result.Entity);
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

            if ((bool)trait.Storage[KEY] == true)
            {
                trait.Storage[KEY] = false;
                owner.Traits.Passives.OnStacksTryToChange.Remove(trait.GuidStr);
                owner.Traits.Actives.OnStacksTryToChange.Remove(trait.GuidStr);
                if (owner.Drawer == null) return;
                _animSeq.Kill();
                owner.Drawer.transform.DOLocalRotate(Vector3.zero, 1).SetEase(Ease.OutQuad);
                if (owner.Data.id == JAKE_CARD_ID)
                {
                    Sprite sprite = Resources.Load<Sprite>(CardBrowser.GetCard(JAKE_CARD_ID).spritePath);
                    owner.Drawer.RedrawPortrait(sprite);
                }
                if (owner.Drawer.PortraitIsFlipped.x)
                    owner.Drawer.FlipPortraitX();
                return;
            }

            trait.Storage[KEY] = true;
            owner.Traits.Passives.OnStacksTryToChange.Add(trait.GuidStr, OnOwnerStacksTryToChange);
            owner.Traits.Actives.OnStacksTryToChange.Add(trait.GuidStr, OnOwnerStacksTryToChange);

            if (owner.Drawer == null) return;
            if (owner.Data.id == JAKE_CARD_ID)
            {
                Sprite sprite = Resources.Load<Sprite>($"Sprites/Cards/Portraits/{JAKE_DANCER_CARD_ID}");
                owner.Drawer.RedrawPortrait(sprite);
            }

            _animSeq = DOTween.Sequence();
            owner.OnDrawerDestroyed += (s, e) => _animSeq.Kill();
            _animSeq.AppendCallback(() =>
            {
                if (owner.Field == null || owner.Drawer == null || !owner.Drawer.gameObject.activeInHierarchy) return;
                float rotation = Utils.RandomIntSafe(10, 30);
                owner.Drawer.FlipPortraitX();
                owner.Drawer.transform.localEulerAngles = new Vector3(0, 0, owner.Drawer.PortraitIsFlipped.x ? rotation : -rotation);
                owner.Drawer.transform.DOAShake();
            });
            _animSeq.AppendInterval(1);
            _animSeq.SetLoops(-1, LoopType.Restart);
            _animSeq.Play();
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;
            if (e.trait.WasAdded(e))
                e.trait.Storage[KEY] = false;
        }

        private async UniTask OnOwnerStacksTryToChange(object sender, TableTraitStacksTryArgs e)
        {
            IBattleTraitList list = (IBattleTraitList)sender;
            BattleFieldCard owner = list.Set.Owner;
            IBattleTrait trait = owner.Traits.Active(ID);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || trait.Owner.Field == null) return;
            await trait.AnimActivationShort();
            if (e.source.AsBattleFieldCard() != owner)
                e.handled = true;
        }
    }
}
