using Game.Cards;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Структура, представляющая "настроение" трейта для улучшения (распределения очков) карты поля (см. <see cref="FieldCard"/>)
    /// </summary>
    public struct TraitMood
    {
        public static readonly TraitMood normal = new()
        {
            healthMod = new TraitMoodMod(0, 1),
            strengthMod = new TraitMoodMod(0, 1),
            moxieReq = TraitMoxieReq.Any,
            traitsMods = new Dictionary<string, TraitMoodMod>(),
        };
        public static TraitMood Custom(TraitMoodMod? healthMod = null, TraitMoodMod? strengthMod = null, TraitMoxieReq? moxieReq = null, Dictionary<string, TraitMoodMod> traitsMods = null)
        {
            TraitMood mood = normal;

            if (healthMod != null) mood.healthMod = (TraitMoodMod)healthMod;
            if (strengthMod != null) mood.strengthMod = (TraitMoodMod)strengthMod;
            if (moxieReq != null) mood.moxieReq = (TraitMoxieReq)moxieReq;
            if (traitsMods != null) mood.traitsMods = traitsMods;

            return mood;
        }

        public TraitMoodMod healthMod;
        public TraitMoodMod strengthMod;
        public TraitMoxieReq moxieReq;
        public IReadOnlyDictionary<string, TraitMoodMod> traitsMods;
    }
}
