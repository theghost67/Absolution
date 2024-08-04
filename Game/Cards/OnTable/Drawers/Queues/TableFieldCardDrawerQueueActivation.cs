using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Palette;
using Game.Traits;
using GreenOne;
using TMPro;
using UnityEngine;
using MyBox;

namespace Game.Cards
{
    public class TableFieldCardDrawerQueueActivation : TableFieldCardDrawerQueueElement
    {
        const float ACTIVATION_DUR_APPEAR = 0.50f;
        const float ACTIVATION_DUR_DISPLAY = 1.00f;
        const float ACTIVATION_DUR_DISAPPEAR = 0.50f;

        // can be set to false (i.e. deactivated)
        public readonly bool activated;
        static readonly GameObject _activationPrefab;

        static TableFieldCardDrawerQueueActivation()
        {
            _activationPrefab = Resources.Load<GameObject>("Prefabs/Traits/Trait activation");
        }
        public TableFieldCardDrawerQueueActivation(ITableTrait trait, bool activated) : base(trait)
        {
            this.activated = activated;
        }

        public override GameObject CreateAnimationPrefab()
        {
            return GameObject.Instantiate(_activationPrefab, trait.Owner.Drawer.transform);
        }
        public override async UniTask PlayAnimation(GameObject prefab)
        {
            if (trait.Owner == null) return;
            if (trait.Owner.Drawer == null) return;

            Trait data = trait.Data;
            Transform prefabTransform = prefab.transform;
            TextMeshPro prefabHeader = prefabTransform.Find<TextMeshPro>("Header");
            SpriteRenderer prefabIcon = prefabTransform.Find<SpriteRenderer>("Icon");
            TextMeshPro prefabSubheader = prefabTransform.Find<TextMeshPro>("Subheader");

            prefabHeader.text = activated ? "активация" : "деактивация"; 
            prefabIcon.sprite = Resources.Load<Sprite>(data.spritePath); // TODO: use traitInList.Drawer.sprite
            prefabSubheader.text = data.name; // TODO: use traitInList.Drawer.text

            Vector3 scale1 = Vector3.one * (activated ? 2.00f : 0.75f);
            Vector3 scale2 = Vector3.one * 1.00f;
            Vector3 scale3 = Vector3.one * 0.75f;

            Color color1 = ColorPalette.C1.ColorCur;
            Color color2 = ColorPalette.All[data.isPassive ? 5 : 6].ColorCur;
            Color color3 = ColorPalette.C2.ColorCur.WithAlpha(0);

            prefabTransform.localScale = scale1;
            prefabHeader.color = color1;
            prefabIcon.color = color1;
            prefabSubheader.color = color1;
            trait.Owner.Drawer.AnimHighlightOutline(1);

            void OnColorTweenUpdate(Color c)
            {
                prefabHeader.color = c;
                prefabIcon.color = c;
                prefabSubheader.color = c;
            }

            Tween colorTween1 = DOVirtual.Color(color1, color2, ACTIVATION_DUR_APPEAR, OnColorTweenUpdate).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween colorTween2 = DOVirtual.Color(color2, color3, ACTIVATION_DUR_DISAPPEAR, OnColorTweenUpdate).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween scaleTween1 = prefabTransform.DOScale(scale2, ACTIVATION_DUR_APPEAR).Pause().SetTarget(prefab).SetEase(Ease.OutCubic);
            Tween scaleTween2 = prefabTransform.DOScale(scale3, ACTIVATION_DUR_DISAPPEAR).Pause().SetTarget(prefab).SetEase(Ease.OutCubic);

            scaleTween1.Play();
            colorTween1.Play();

            await UniTask.Delay((int)(ACTIVATION_DUR_DISPLAY * 1000));

            scaleTween2.Play();
            colorTween2.Play();

            await UniTask.Delay((int)(ACTIVATION_DUR_DISAPPEAR * 1000));

            prefab.Destroy();
        }
    }
}
