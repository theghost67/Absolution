using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Palette;
using Game.Traits;
using GreenOne;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using MyBox;

namespace Game.Cards
{
    public class TableFieldCardDrawerQueueDetection : TableFieldCardDrawerQueueElement
    {
        const float ACTIVATION_DUR_APPEAR = 0.50f;
        const float ACTIVATION_DUR_DISPLAY = 1.00f;
        const float ACTIVATION_DUR_DISAPPEAR = 0.50f;

        public readonly int2 pos;
        public readonly bool canSee;
        static readonly GameObject _detectionPrefab;

        static TableFieldCardDrawerQueueDetection()
        {
            _detectionPrefab = Resources.Load<GameObject>("Prefabs/Traits/Trait detection");
        }
        public TableFieldCardDrawerQueueDetection(ITableTrait trait, int2 pos, bool canSee) : base(trait)
        {
            this.pos = pos;
            this.canSee = canSee;
        }

        public override GameObject CreateAnimationPrefab()
        {
            return GameObject.Instantiate(_detectionPrefab, trait.Owner.Drawer.transform);
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

            prefabHeader.text = canSee ? "цель обнаружена" : "цель потеряна"; 
            prefabIcon.sprite = Resources.Load<Sprite>(data.spritePath); // TODO: use traitInList.Drawer.sprite
            prefabSubheader.text = data.name; // TODO: use traitInList.Drawer.text

            Vector3 scale1 = Vector3.one * 0.75f;
            Vector3 scale2 = Vector3.one * 1.00f;
            Vector3 scale3 = Vector3.one * 0.75f;

            Color color1 = ColorPalette.C1.ColorCur;
            Color color2 = ColorPalette.All[data.isPassive ? 5 : 6].ColorCur;
            Color color3 = ColorPalette.C2.ColorCur.WithAlpha(0);

            prefabTransform.localScale = scale1;
            prefabHeader.color = color1;
            prefabIcon.color = color2;
            prefabSubheader.color = color1;
            trait.Owner.Drawer.AnimHighlightOutline(1);

            void OnColorTweenUpdate(Color c)
            {
                prefabHeader.color = prefabHeader.color.WithAlpha(c.a);
                prefabIcon.color = c;
                prefabSubheader.color = prefabHeader.color.WithAlpha(c.a);
            }

            Tween colorTween1 = DOVirtual.Color(color2, color3, ACTIVATION_DUR_DISAPPEAR, OnColorTweenUpdate).Pause().SetTarget(prefab).SetEase(Ease.Linear);
            Tween scaleTween1 = prefabTransform.DOScale(scale2, ACTIVATION_DUR_APPEAR).Pause().SetTarget(prefab).SetEase(Ease.OutCubic);
            Tween scaleTween2 = prefabTransform.DOScale(scale3, ACTIVATION_DUR_DISAPPEAR).Pause().SetTarget(prefab).SetEase(Ease.OutCubic);

            scaleTween1.Play();
            await UniTask.Delay((int)(ACTIVATION_DUR_DISPLAY * 1000));
            colorTween1.Play();
            scaleTween2.Play();
            prefab.Destroy(ACTIVATION_DUR_DISAPPEAR);
        }
    }
}
