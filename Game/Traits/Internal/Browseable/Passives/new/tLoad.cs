using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using GreenOne;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tLoad : PassiveTrait
    {
        const string ID = "load";
        const string KEY = "load";
        const int TURNS = 3;

        public tLoad() : base(ID)
        {
            name = Translator.GetString("trait_load_1");
            desc = Translator.GetString("trait_load_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tLoad(tLoad other) : base(other) { }
        public override object Clone() => new tLoad(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_load_3", TURNS);

        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Storage[KEY] = 0;
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryOnStartPhase);
                trait.Owner.OnDrawerCreated += OnOwnerDrawerCreated;
            }
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        void OnOwnerDrawerCreated(object sender, EventArgs e)
        {
            TableObject obj = (TableObject)sender;
            BattleFieldCard owner = (BattleFieldCard)obj.Drawer.attached;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait.Owner.Drawer == null) return;
            if (trait.Owner.Data.id != "bmo") return;
            int index = Utils.RandomIntSafe(0, 6);
            if (index == 0) return;
            Sprite sprite = Resources.Load<Sprite>($"Sprites/Cards/Portraits/bmo_{index}");
            trait.Owner.OnDrawerCreated += (s, e) => trait.Owner.Drawer.RedrawPortrait(sprite);
        }

        async UniTask OnTerritoryOnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled || !trait.Storage.TryGetValue(KEY, out object turns)) return;

            int turnsInt = (int)turns + 1;
            if (turnsInt < TURNS)
            {
                trait.Storage[KEY] = turnsInt;
                return;
            }

            trait.Storage[KEY] = 0;
            await trait.AnimActivation();
            await trait.Side.Health.AdjustValue(trait.Owner.Health, trait);
        }
    }
}
