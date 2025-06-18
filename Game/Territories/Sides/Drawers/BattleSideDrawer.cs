using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Sleeves;
using GreenOne;
using TMPro;
using UnityEngine;

namespace Game.Territories
{
    /// <summary>
    /// Представляет отрисовщик UI для <see cref="BattleSide"/>.
    /// </summary>
    public class BattleSideDrawer : Drawer
    {
        public bool WealthIsVisible
        {
            get => _wealthIsVisibleCounter >= 0;
            set
            {
                if (value)
                    _wealthIsVisibleCounter++;
                else _wealthIsVisibleCounter--;

                WealthSetActive(WealthIsVisible);
            }
        }
        public bool SleeveIsVisible
        {
            get => _sleeveIsVisibleCounter >= 0;
            set
            {
                if (value)
                    _sleeveIsVisibleCounter++;
                else _sleeveIsVisibleCounter--;

                SleeveSetActive(SleeveIsVisible);
            }
        }
        public readonly new BattleSide attached;

        static readonly GameObject _playerPrefab;
        static readonly GameObject _enemyPrefab;

        static readonly Vector2 _playerHpBarMaskMinMaxX = new(-1.40f, 0f);
        static readonly Vector2 _enemyHpBarMaskMinMaxX = new(1.40f, 0f);

        static readonly Sprite _playerGoldPanelUnlockedSprite;
        static readonly Sprite _playerGoldPanelLockedSprite;
        static readonly Sprite _playerEtherPanelUnlockedSprite;
        static readonly Sprite _playerEtherPanelLockedSprite;

        static readonly Sprite _enemyGoldPanelUnlockedSprite;
        static readonly Sprite _enemyGoldPanelLockedSprite;
        static readonly Sprite _enemyEtherPanelUnlockedSprite;
        static readonly Sprite _enemyEtherPanelLockedSprite;

        readonly Transform _healthBarMask;
        readonly Vector2 _hpBarMinMaxX;

        readonly Transform _healthPanel;
        readonly SpriteRenderer _goldPanel;
        readonly SpriteRenderer _etherPanel;

        readonly TextMeshPro _healthText;
        readonly TextMeshPro _goldText;
        readonly TextMeshPro _etherText;

        readonly string _eventsGuid;

        int _gold;
        int _ether;

        int _wealthIsVisibleCounter;
        int _sleeveIsVisibleCounter;

        Tween _goldTween;
        Tween _etherTween;

        static BattleSideDrawer()
        {
            _playerPrefab = Resources.Load<GameObject>("Prefabs/Territories/Player side");
            _enemyPrefab = Resources.Load<GameObject>("Prefabs/Territories/Enemy side");

            _playerGoldPanelUnlockedSprite  = Resources.Load<Sprite>("Sprites/Menus/battle/Player gold");
            _playerGoldPanelLockedSprite    = Resources.Load<Sprite>("Sprites/Menus/battle/Player gold locked");
            _playerEtherPanelUnlockedSprite = Resources.Load<Sprite>("Sprites/Menus/battle/Player ether");
            _playerEtherPanelLockedSprite   = Resources.Load<Sprite>("Sprites/Menus/battle/Player ether locked");
            _enemyGoldPanelUnlockedSprite   = Resources.Load<Sprite>("Sprites/Menus/battle/Enemy gold");
            _enemyGoldPanelLockedSprite     = Resources.Load<Sprite>("Sprites/Menus/battle/Enemy gold locked");
            _enemyEtherPanelUnlockedSprite  = Resources.Load<Sprite>("Sprites/Menus/battle/Enemy ether");
            _enemyEtherPanelLockedSprite    = Resources.Load<Sprite>("Sprites/Menus/battle/Enemy ether locked");
        }
        public BattleSideDrawer(BattleSide side, Transform parent) : base(side, side.isMe ? _playerPrefab : _enemyPrefab, parent)
        {
            attached = side;
            _eventsGuid = this.GuidGen(1);

            _healthBarMask = transform.Find("Hp").Find("Bar mask");
            _healthPanel = transform.Find("Hp");
            _goldPanel = transform.Find<SpriteRenderer>("Gold");
            _etherPanel = transform.Find<SpriteRenderer>("Ether");

            _hpBarMinMaxX = side.isMe ? _playerHpBarMaskMinMaxX : _enemyHpBarMaskMinMaxX;
            _healthText = _healthPanel.transform.Find<TextMeshPro>("Text");
            _goldText = _goldPanel.transform.Find<TextMeshPro>("Text");
            _etherText = _etherPanel.transform.Find<TextMeshPro>("Text");

            side.Health.OnPostSet.Add(_eventsGuid, OnHealthPostSet);
            side.Gold.OnPostSet.Add(_eventsGuid, OnGoldPostSet);
            side.Ether.OnPostSet.Add(_eventsGuid, OnEtherPostSet);

            if (!side.isMe)
            {
                WealthIsVisible = false;
                SleeveIsVisible = false;
            }

            RedrawHealth();
            RedrawGold();
            RedrawEther();
        }

        public void RedrawHealth()
        {
            RedrawHealth(attached.Health, attached.HealthAtStart);
        }
        public void RedrawGold()
        {
            RedrawGold(attached.Gold);
        }
        public void RedrawEther()
        {
            RedrawEther(attached.Ether);
        }

        public void RedrawHealth(int current, int max)
        {
            if (!attached.isMe)
                _healthText.text = $"{current}/{max}";
            else if (PlayerConfig.psychoMode && PlayerConfig.chaosMode)
                _healthText.text = Translator.GetString("battle_side_drawer_1");
            else if (PlayerConfig.psychoMode)
                 _healthText.text = Translator.GetString("battle_side_drawer_2");
            else if (PlayerConfig.chaosMode)
                 _healthText.text = Translator.GetString("battle_side_drawer_3", current, max);
            else _healthText.text = $"{current}/{max}";
            AnimHealthBar(current, max);
        }
        public void RedrawGold(int value)
        {
            _goldTween.Kill(complete: true);
            _goldTween = _goldText.DOATextNumberDelta(_gold, value, 1);
            _gold = value;
        }
        public void RedrawEther(int value)
        {
            _etherTween.Kill(complete: true);
            _etherTween = _etherText.DOATextNumberDelta(_ether, value, 1);
            _ether = value;
        }

        protected override void DestroyInstantly()
        {
            base.DestroyInstantly();
            attached.Health.OnPostSet.Remove(_eventsGuid);
            attached.Gold.OnPostSet.Remove(_eventsGuid);
            attached.Ether.OnPostSet.Remove(_eventsGuid);
        }

        UniTask OnHealthPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;
            side.Drawer?.RedrawHealth();
            return UniTask.CompletedTask;
        }
        UniTask OnGoldPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;
            side.Drawer?.RedrawGold();
            return UniTask.CompletedTask;
        }
        UniTask OnEtherPostSet(object sender, TableStat.PostSetArgs e)
        {
            TableStat stat = (TableStat)sender;
            BattleSide side = (BattleSide)stat.Owner;
            side.Drawer?.RedrawEther();
            return UniTask.CompletedTask;
        }

        void WealthSetActive(bool value)
        {
            _goldPanel.gameObject.SetActive(value);
            _etherPanel.gameObject.SetActive(value);
            if (attached.isMe)
            {
                _goldPanel.sprite  = value ? _playerGoldPanelUnlockedSprite  : _playerGoldPanelLockedSprite;
                _etherPanel.sprite = value ? _playerEtherPanelUnlockedSprite : _playerEtherPanelLockedSprite;
            }
            else
            {
                _goldPanel.sprite  = value ? _enemyGoldPanelUnlockedSprite  : _enemyGoldPanelLockedSprite;
                _etherPanel.sprite = value ? _enemyEtherPanelUnlockedSprite : _enemyEtherPanelLockedSprite;
            }
        }
        void SleeveSetActive(bool value)
        {
            BattleSleeveDrawer drawer = attached.Sleeve?.Drawer;
            if (drawer == null) return;
            if (value)
                 drawer.MoveIn();
            else drawer.MoveOut();
        }

        void AnimHealthBar(int currentHp, int maxHp)
        {
            float ratio = (float)currentHp / maxHp;
            float newX = Mathf.Lerp(_hpBarMinMaxX.x, _hpBarMinMaxX.y, ratio);
            _healthBarMask.transform.DOLocalMoveX(newX, 0.75f).SetEase(Ease.OutCubic);
        }
    }
}
