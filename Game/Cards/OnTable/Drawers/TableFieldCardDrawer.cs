using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Palette;
using Game.Traits;
using GreenOne;
using MyBox;
using UnityEngine;

namespace Game.Cards
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableFieldCard"/>.
    /// </summary>
    public class TableFieldCardDrawer : TableCardDrawer
    {
        const float BG_ALPHA_MAX = 0.8f;

        static Color _outlineBrightPassiveColor;
        static Color _outlineBrightActiveColor;

        static Color _outlineDimPassiveColor;
        static Color _outlineDimActiveColor;

        public TableTraitListSetDrawer Traits => attached.Traits.Drawer;
        public readonly new TableFieldCard attached;

        readonly SpriteRenderer _bgRenderer;
        Tween _bgTween;

        enum OutlineType
        {
            None,
            Passive,
            Active,
            Both
        }

        static TableFieldCardDrawer()
        {
            OnPaletteColorChanged_Static(ColorPalette.PASSIVE_INDEX);
            OnPaletteColorChanged_Static(ColorPalette.ACTIVE_INDEX);
            ColorPalette.OnColorChanged += OnPaletteColorChanged_Static;
        }
        public TableFieldCardDrawer(TableFieldCard card, Transform parent) : base(card, parent)
        {
            attached = card;
            attached.price.OnPostSet.Add(OnPriceStatPostSet);
            attached.moxie.OnPostSet.Add(OnMoxieStatPostSet);
            attached.health.OnPostSet.Add(OnHealthStatPostSet);
            attached.strength.OnPostSet.Add(OnStrengthStatPostSet);
            attached.Traits.Passives.OnStacksChanged.Add(OnTraitsStacksChanged);
            attached.Traits.Actives.OnStacksChanged.Add(OnTraitsStacksChanged);

            ColorPalette.OnColorChanged += OnPaletteColorChanged;
            OnMouseScrollUp += OnMouseScrollUpBase;
            OnMouseScrollDown += OnMouseScrollDownBase;

            _bgRenderer = transform.Find<SpriteRenderer>("BG");
            CreateOutline();
            RedrawFieldData();
            RedrawOutlineInstantly();
        }

        public override void SetCollider(bool value)
        {
            base.SetCollider(value);
            Traits?.SetCollider(value);
        }
        public override void SetSortingOrder(int value, bool asDefault = false)
        {
            base.SetSortingOrder(value, asDefault);
            _bgRenderer.sortingOrder = value + 2;
            Traits?.SetSortingOrder(value + 3);
        }
        public override void SetAlpha(float value)
        {
            base.SetAlpha(value);
            if (_bgRenderer.color.a != 0)
                _bgRenderer.SetAlpha(value * BG_ALPHA_MAX);
            Traits.SetAlpha(value);
        }
        public override void SetColor(Color value)
        {
            base.SetColor(value);
            if (_bgRenderer.color.a != 0)
                _bgRenderer.color = value * BG_ALPHA_MAX;
            Traits.SetColor(value);
        }

        public void RedrawHealthAsDefault()
        {
            RedrawHealth(attached.Data.health);
        }
        public void RedrawStrengthAsDefault()
        {
            RedrawStrength(attached.Data.strength);
        }
        public void RedrawMoxieAsDefault()
        {
            RedrawMoxie(attached.Data.moxie);
        }

        public void RedrawHealthAsCurrent()
        {
            RedrawHealth(attached.health);
        }
        public void RedrawStrengthAsCurrent()
        {
            RedrawStrength(attached.strength);
        }
        public void RedrawMoxieAsCurrent()
        {
            RedrawMoxie(attached.moxie);
        }

        public void RedrawHealth(int health)
        {
            lowerLeftIcon.RedrawSprite(lHealthIconSprite);
            lowerLeftIcon.RedrawText(health);
        }
        public void RedrawStrength(int strength)
        {
            lowerRightIcon.RedrawSprite(lStrengthIconSprite);
            lowerRightIcon.RedrawText(strength);
        }
        public void RedrawMoxie(int moxie)
        {
            upperRightIcon.RedrawSprite(uMoxieIconSprite);
            upperRightIcon.RedrawChunks(moxie);
        }

        public void ShowBg()
        {
            _bgTween?.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(BG_ALPHA_MAX), 0.25f).SetEase(Ease.Linear);
        }
        public void HideBg()
        {
            _bgTween?.Kill();
            _bgTween = _bgRenderer.DOColor(Color.white.WithAlpha(0f), 0.25f).SetEase(Ease.Linear);
        }

        public void ShowBgInstantly()
        {
            _bgTween?.Kill();
            _bgRenderer.color = Color.white.WithAlpha(BG_ALPHA_MAX);
        }
        public void HideBgInstantly()
        {
            _bgTween?.Kill();
            _bgRenderer.color = Color.white.WithAlpha(0f);
        }

        public void HighlightOutline(Color color, float duration = 1)
        {
            Color prevColor = Outline.GetColor();
            float brightF = Mathf.Pow(2, 6);

            color.r *= brightF;
            color.g *= brightF;
            color.b *= brightF;

            if (duration == -1)
                duration = 1;

            Outline.SetColor(color);
            Outline.TweenColor(prevColor, duration).SetEase(Ease.InOutQuad).OnComplete(RedrawOutline);
        }
        public void HighlightOutline(bool asPassive, float duration = 1)
        {
            if (asPassive)
            {
                Outline.SetColor(_outlineBrightPassiveColor);
                Outline.TweenColor(_outlineDimPassiveColor, duration).SetEase(Ease.OutQuad).OnComplete(RedrawOutline);
            }
            else
            {
                Outline.SetColor(_outlineBrightActiveColor);
                Outline.TweenColor(_outlineDimActiveColor, duration).SetEase(Ease.OutQuad).OnComplete(RedrawOutline);
            }
        }

        protected void RedrawOutline()
        {
            const float DURATION = 1.5f;
            switch (GetOutlineType())
            {
                case OutlineType.Passive: Outline.TweenColor(_outlineDimPassiveColor, DURATION); break;
                case OutlineType.Active: Outline.TweenColor(_outlineDimActiveColor, DURATION); break;
                case OutlineType.Both: Outline.TweenColorLerpEndless(_outlineDimPassiveColor, _outlineDimActiveColor, DURATION, DURATION); break;

                default:
                    if (Outline.GetColor().a != 0)
                        Outline.TweenColor(Color.clear, DURATION);
                    break;
            }
        }
        protected void RedrawOutlineInstantly()
        {
            const float DURATION = 1.5f;
            switch (GetOutlineType())
            {
                case OutlineType.Passive: Outline.SetColor(_outlineDimPassiveColor); break;
                case OutlineType.Active: Outline.SetColor(_outlineDimActiveColor); break;
                case OutlineType.Both: Outline.TweenColorLerpEndless(_outlineDimPassiveColor, _outlineDimActiveColor, 0, DURATION); break;

                default:
                    if (Outline.GetColor().a != 0)
                        Outline.SetColor(Color.clear);
                    break;
            }
        }

        protected virtual bool RedrawRangeFlipY() => true;
        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();

            ColorPalette.OnColorChanged -= OnPaletteColorChanged;

            attached.price.OnPostSet.Remove(OnPriceStatPostSet);
            attached.moxie.OnPostSet.Remove(OnMoxieStatPostSet);
            attached.health.OnPostSet.Remove(OnHealthStatPostSet);
            attached.strength.OnPostSet.Remove(OnStrengthStatPostSet);
            attached.Traits.Passives.OnStacksChanged.Remove(OnTraitsStacksChanged);
            attached.Traits.Actives.OnStacksChanged.Remove(OnTraitsStacksChanged);
        }

        protected override void OnUpperLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return;
            CardCurrency priceCurrency = attached.Data.price.currency;
            string priceCurrencyStr = priceCurrency.name.Colored(priceCurrency.color);

            int priceDefault = attached.Data.price.value;
            int priceCurrent = attached.price;
            Tooltip.Show($"Валюта: {priceCurrencyStr}\nПо умолчанию: {priceDefault} ед.\nТекущее: {priceCurrent} ед.\n<color=grey><i>Стоимость: цена установки на территорию.");
        }
        protected override void OnUpperRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int moxieDefault = attached.Data.moxie;
            int moxieCurrent = attached.moxie;
            Tooltip.Show($"По умолчанию: {moxieDefault} ед.\nТекущее: {moxieCurrent} ед.\n<color=grey><i>Инициатива: определяет быстроту действий.");
        }
        protected override void OnLowerLeftIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int healthDefault = attached.Data.health;
            int healthCurrent = attached.health;
            Tooltip.Show($"По умолчанию: {healthDefault} ед.\nТекущее: {healthCurrent} ед.\n<color=grey><i>Здоровье: по достижении нуля наступает смерть.");
        }
        protected override void OnLowerRightIconMouseEnter(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            int strengthDefault = attached.Data.strength;
            int strengthCurrent = attached.strength;
            Tooltip.Show($"По умолчанию: {strengthDefault} ед.\nТекущее: {strengthCurrent} ед.\n<color=grey><i>Сила: наносимый урон здоровью собственными атаками.");
        }

        protected override void OnUpperLeftIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }
        protected override void OnUpperRightIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }
        protected override void OnLowerLeftIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }
        protected override void OnLowerRightIconMouseLeave(object sender, DrawerMouseEventArgs e)
        {
            if (e.handled) return; 
            Tooltip.Hide();
        }

        protected override void OnMouseEnterBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseEnterBase(sender, e);
            if (e.handled) return;
            Traits?.ShowStoredElementsInstantly();
        }
        protected override void OnMouseLeaveBase(object sender, DrawerMouseEventArgs e)
        {
            base.OnMouseLeaveBase(sender, e);
            if (e.handled) return;

            RedrawHealthAsCurrent();
            RedrawStrengthAsCurrent();
            Traits?.HideStoredElementsInstantly();
        }
        protected override void OnMouseClickLeftBase(object sender, DrawerMouseEventArgs e)
        {
            e.handled = Traits.elements.IsAnySelected;
            base.OnMouseClickLeftBase(sender, e);
            if (e.handled) return;
        }

        static void OnPaletteColorChanged_Static(int index)
        {
            const int PASSIVE = ColorPalette.PASSIVE_INDEX;
            const int ACTIVE = ColorPalette.ACTIVE_INDEX;
            float brightF = Mathf.Pow(2, 6); // intensity = 6

            if (index == PASSIVE)
            {
                Color color = ColorPalette.GetColor(PASSIVE);
                float grayscale = color.grayscale;
                color = ((1 - grayscale) * Color.white + color * grayscale).WithAlpha(1);

                _outlineBrightPassiveColor = (color * brightF).WithAlpha(1);
                _outlineDimPassiveColor    = color;
            }
            else if (index == ACTIVE)
            {
                Color color = ColorPalette.GetColor(ACTIVE);
                float grayscale = color.grayscale;
                color = ((1 - grayscale) * Color.white + color * grayscale).WithAlpha(1);

                _outlineBrightActiveColor = (color * brightF).WithAlpha(1);
                _outlineDimActiveColor = color;
            }
        }
        void OnPaletteColorChanged(int index)
        {
            RedrawOutlineInstantly();
        }

        UniTask OnTraitsStacksChanged(object sender, TableTraitStacksSetArgs e)
        {
            TableTraitList list = (TableTraitList)sender;
            TableFieldCard card = list.Set.Owner;

            TableFieldCardDrawer drawer = card.Drawer;
            drawer?.RedrawOutline();
            return UniTask.CompletedTask;
        }
        UniTask OnPriceStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.RedrawPriceAsDefault();
            return UniTask.CompletedTask;
        }
        UniTask OnMoxieStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            card.Drawer?.RedrawMoxieAsCurrent();
            return UniTask.CompletedTask;
        }
        UniTask OnHealthStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            TableFieldCardDrawer drawer = card.Drawer;
            drawer?.RedrawHealthAsCurrent();
            return UniTask.CompletedTask;
        }
        UniTask OnStrengthStatPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            TableFieldCard card = (TableFieldCard)stat.Owner;
            TableFieldCardDrawer drawer = card.Drawer;
            drawer?.RedrawStrengthAsCurrent();
            return UniTask.CompletedTask;
        }

        OutlineType GetOutlineType()
        {
            TableTraitListSet traits = attached.Traits;
            bool hasPassives = traits.Passives.Count != 0;
            bool hasActives = traits.Actives.Count != 0;

            if (hasPassives == hasActives)
            {
                if (hasPassives)
                     return OutlineType.Both;
                else return OutlineType.None;
            }
            if (hasPassives)
                 return OutlineType.Passive;
            else return OutlineType.Active;
        }
        void RedrawFieldData()
        {
            FieldCard cardData = attached.Data;
            RedrawHealth(cardData.health);
            RedrawStrength(cardData.strength);
            RedrawMoxie(cardData.moxie);
        }
    }
}
