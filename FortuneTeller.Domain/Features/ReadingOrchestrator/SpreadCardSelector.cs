using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Domain.Features.ReadingOrchestrator;

public class SpreadCardSelector
{
    private static readonly List<TarotCardModel> FullDeck = InitializeDeck();

    public IReadOnlyList<TarotCardModel> GetAllCards() => FullDeck.AsReadOnly();

    public TarotSpreadInfoModel GetSpreadInfo(TarotSpreadType spreadType)
    {
        return spreadType switch
        {
            TarotSpreadType.SingleCard => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.SingleCard,
                Title = "The Oracle's Eye (Single Card)",
                Description = "A laser-focused beacon of cosmic truth for instant clarity or daily guidance.",
                CardCount = 1,
                Positions = new() { "The Core Revelation" }
            },
            TarotSpreadType.ThreeCards => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.ThreeCards,
                Title = "Trinity of Time (Three Cards)",
                Description = "Illuminates the threads connecting past foundations, present challenges, and future destiny.",
                CardCount = 3,
                Positions = new() { "The Past (Roots)", "The Present (Current Energy)", "The Future (Destiny's Horizon)" }
            },
            TarotSpreadType.FiveCards => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.FiveCards,
                Title = "The Celestial Pentagram (Five Cards)",
                Description = "A profound esoteric map encompassing Spirit, Fire, Water, Air, and Earth energies.",
                CardCount = 5,
                Positions = new()
                {
                    "1. The Seeker's Soul (Spirit)",
                    "2. Hidden Obstacles (Shadow)",
                    "3. Divine Allies & Strength (Fire)",
                    "4. Subconscious Wisdom (Water)",
                    "5. Ultimate Prophecy (Destiny)"
                }
            },
            _ => GetSpreadInfo(TarotSpreadType.ThreeCards)
        };
    }

    public List<SelectedCardModel> SelectSpreadAndCardsInMemory(
        TarotSpreadType spreadType,
        List<int>? userSelectedCardIds = null)
    {
        var spreadInfo = GetSpreadInfo(spreadType);
        var rng = new Random();
        var selected = new List<SelectedCardModel>();
        var chosenCards = new List<TarotCardModel>();

        if (userSelectedCardIds != null && userSelectedCardIds.Count >= spreadInfo.CardCount)
        {
            foreach (var id in userSelectedCardIds.Take(spreadInfo.CardCount))
            {
                var card = FullDeck.FirstOrDefault(c => c.Id == id) ?? FullDeck[rng.Next(FullDeck.Count)];
                chosenCards.Add(card);
            }
        }
        else
        {
            // Shuffle in memory and draw
            var shuffled = FullDeck.OrderBy(_ => rng.Next()).ToList();
            chosenCards = shuffled.Take(spreadInfo.CardCount).ToList();
        }

        for (int i = 0; i < chosenCards.Count; i++)
        {
            var isReversed = rng.Next(0, 100) < 25; // 25% chance of reversed orientation
            var posName = i < spreadInfo.Positions.Count ? spreadInfo.Positions[i] : $"Position {i + 1}";

            selected.Add(new SelectedCardModel
            {
                Card = chosenCards[i],
                PositionIndex = i + 1,
                PositionName = posName,
                IsReversed = isReversed
            });
        }

        return selected;
    }

    private static List<TarotCardModel> InitializeDeck()
    {
        return new List<TarotCardModel>
        {
            new() {
                Id = 0, Name = "The Fool", Arcana = "Major", Number = 0, Element = "Air", AstrologicalSign = "Uranus",
                Symbol = "🌌", Keywords = "Beginnings, Innocence, Spontaneity, Leap of Faith",
                UprightMeaning = "A glorious new dawn beckons. Step fearlessly into the unknown with an unburdened heart.",
                ReversedMeaning = "Hesitation or recklessness. Look carefully before taking an impulsive leap.",
                Description = "A traveler on the precipice of infinity, accompanied by a white dog, ready to explore the cosmos."
            },
            new() {
                Id = 1, Name = "The Magician", Arcana = "Major", Number = 1, Element = "Air", AstrologicalSign = "Mercury",
                Symbol = "✨", Keywords = "Manifestation, Power, Resourcefulness, Alchemy",
                UprightMeaning = "All four celestial elements align at your command. You possess the raw willpower to manifest your vision.",
                ReversedMeaning = "Untapped cosmic potential or deceptive illusions. Channel your focus cleanly.",
                Description = "Standing beneath an infinity symbol, raising a wand to the heavens with tools of earth, fire, air, and water."
            },
            new() {
                Id = 2, Name = "The High Priestess", Arcana = "Major", Number = 2, Element = "Water", AstrologicalSign = "Moon",
                Symbol = "🔮", Keywords = "Intuition, Sacred Secrets, Subconscious, Serenity",
                UprightMeaning = "The veil between worlds grows thin. Trust your deepest psychic whispers over cold analytical logic.",
                ReversedMeaning = "Ignoring your inner divine compass or superficial gossip clouding pure insight.",
                Description = "Seated between pillars of darkness and light, draped in sapphire robes with the crescent moon at her feet."
            },
            new() {
                Id = 3, Name = "The Empress", Arcana = "Major", Number = 3, Element = "Earth", AstrologicalSign = "Venus",
                Symbol = "🌿", Keywords = "Abundance, Fertility, Nurturing, Creative Bloom",
                UprightMeaning = "Golden harvests and luscious blessings surround your endeavors. What you cultivate will flourish.",
                ReversedMeaning = "Creative blockage or feeling depleted. Reconnect with Mother Nature's healing energies.",
                Description = "Crowned with twelve celestial stars, resting amidst golden wheat fields and cascading crystal waters."
            },
            new() {
                Id = 4, Name = "The Emperor", Arcana = "Major", Number = 4, Element = "Fire", AstrologicalSign = "Aries",
                Symbol = "👑", Keywords = "Authority, Structure, Sovereign Will, Stability",
                UprightMeaning = "Foundations carved in stone. Lead with calm firmness and establish order over chaotic currents.",
                ReversedMeaning = "Rigidity, stubborn resistance to change, or feelings of disempowerment.",
                Description = "A commanding sovereign seated upon a stone throne adorned with carved rams' heads."
            },
            new() {
                Id = 5, Name = "The Hierophant", Arcana = "Major", Number = 5, Element = "Earth", AstrologicalSign = "Taurus",
                Symbol = "📜", Keywords = "Spiritual Wisdom, Tradition, Mentorship, Sacred Truth",
                UprightMeaning = "Ancient lineages of wisdom offer sanctuary. Seek counsel from revered teachers or ancestral roots.",
                ReversedMeaning = "Outdated dogma no longer serving your soul. Forge your own unorthodox path.",
                Description = "A sacred pontiff blessing seekers between twin pillars of divine knowledge."
            },
            new() {
                Id = 6, Name = "The Lovers", Arcana = "Major", Number = 6, Element = "Air", AstrologicalSign = "Gemini",
                Symbol = "💖", Keywords = "Sacred Union, Harmony, Values Alignment, Choice",
                UprightMeaning = "A profound convergence of souls or a vital fork in destiny demanding moral integrity.",
                ReversedMeaning = "Inner discord, conflicting desires, or imbalance between passion and reason.",
                Description = "Two souls blessed by the angel Raphael under the radiance of the celestial sun."
            },
            new() {
                Id = 7, Name = "The Chariot", Arcana = "Major", Number = 7, Element = "Water", AstrologicalSign = "Cancer",
                Symbol = "⚡", Keywords = "Triumph, Momentum, Directed Will, Victory",
                UprightMeaning = "Harnessing opposing cosmic forces into triumphant forward propulsion. Victory is assured.",
                ReversedMeaning = "Feeling pulled in divergent directions. Regain rein over your emotional steeds.",
                Description = "A triumphant warrior armored in starlight driving twin sphinxes of black and white."
            },
            new() {
                Id = 8, Name = "Strength", Arcana = "Major", Number = 8, Element = "Fire", AstrologicalSign = "Leo",
                Symbol = "🦁", Keywords = "Compassion, Gentle Courage, Mastery, Grace",
                UprightMeaning = "Gentle compassion tames the fiercest cosmic storms. True power whispers rather than roars.",
                ReversedMeaning = "Self-doubt or letting primal anger overcome your spiritual composure.",
                Description = "A maiden adorned with flowers serenely closing the jaws of a majestic golden lion."
            },
            new() {
                Id = 9, Name = "The Hermit", Arcana = "Major", Number = 9, Element = "Earth", AstrologicalSign = "Virgo",
                Symbol = "🏮", Keywords = "Solitude, Inner Illumination, Guidance, Soul Searching",
                UprightMeaning = "Withdraw from the sensory noise of the mundane world. Your inner beacon holds the sacred coordinates.",
                ReversedMeaning = "Excessive isolation or fear of looking within. Let your light shine outward.",
                Description = "An elder holding an amber lantern containing a six-pointed star of truth upon a mountain peak."
            },
            new() {
                Id = 10, Name = "Wheel of Fortune", Arcana = "Major", Number = 10, Element = "Fire", AstrologicalSign = "Jupiter",
                Symbol = "🎡", Keywords = "Karma, Serendipity, Cycles of Fate, Golden Turning",
                UprightMeaning = "The celestial wheel spins in your favor! Serendipity and karmic wind fill your sails.",
                ReversedMeaning = "A transient downturn in the cosmic cycle. Remember that all phases are temporary.",
                Description = "The four winged creatures of destiny spinning an esoteric wheel inscribed with Hebrew letters."
            },
            new() {
                Id = 11, Name = "Justice", Arcana = "Major", Number = 11, Element = "Air", AstrologicalSign = "Libra",
                Symbol = "⚖️", Keywords = "Fairness, Karmic Equilibrium, Truth, Clarity",
                UprightMeaning = "Cosmic balance is restored. Truth cuts through pretense like a double-edged astral blade.",
                ReversedMeaning = "Perceived unfairness or dodging accountability. Align with unwavering integrity.",
                Description = "The divine arbiter holding upright scales of balance and the sword of unclouded discernment."
            },
            new() {
                Id = 12, Name = "The Hanged Man", Arcana = "Major", Number = 12, Element = "Water", AstrologicalSign = "Neptune",
                Symbol = "🕊️", Keywords = "Surrender, New Perspective, Enlightened Pause",
                UprightMeaning = "Sacrifice ego-driven haste. By inverting your viewpoint, celestial enlightenment is revealed.",
                ReversedMeaning = "Fruitless stalling or resistance to surrendering what has expired.",
                Description = "Suspended gracefully upside down from the living Tree of Life, glowing with a golden halo."
            },
            new() {
                Id = 13, Name = "Death (Metamorphosis)", Arcana = "Major", Number = 13, Element = "Water", AstrologicalSign = "Scorpio",
                Symbol = "🦋", Keywords = "Transformation, Rebirth, Closure, Phoenix Rise",
                UprightMeaning = "The old husk shatters so the immortal soul can unfurl radiant wings. Profound spiritual rebirth.",
                ReversedMeaning = "Clinging to dead branches. Release your grip and embrace the miraculous dawn.",
                Description = "A dark armored rider bearing a mystical white rose, with the twin towers of dawn rising beyond."
            },
            new() {
                Id = 14, Name = "Temperance", Arcana = "Major", Number = 14, Element = "Fire", AstrologicalSign = "Sagittarius",
                Symbol = "🏺", Keywords = "Alchemy, Balance, Divine Moderation, Healing",
                UprightMeaning = "Blending disparate energies into gold. Sacred harmony and steady healing restore your spirit.",
                ReversedMeaning = "Extremes, impatience, or discord. Seek the calm center of the storm.",
                Description = "An angel pouring liquid fire and water seamlessly between two golden chalices."
            },
            new() {
                Id = 15, Name = "The Devil", Arcana = "Major", Number = 15, Element = "Earth", AstrologicalSign = "Capricorn",
                Symbol = "⛓️", Keywords = "Shadow Work, Illusions, Liberation, Breaking Chains",
                UprightMeaning = "Confront the shadow's illusory grip. Recognize that the chains binding you are loose enough to slip off.",
                ReversedMeaning = "Breaking free from toxic cycles, awakening from spiritual slumber.",
                Description = "A horned figure presiding over captive souls whose collars are loose enough to be cast away."
            },
            new() {
                Id = 16, Name = "The Tower", Arcana = "Major", Number = 16, Element = "Fire", AstrologicalSign = "Mars",
                Symbol = "⚡", Keywords = "Sudden Breakthrough, Shattered Illusions, Awakening",
                UprightMeaning = "Celestial lightning topples false foundations. Painful yet liberating truth clears holy ground.",
                ReversedMeaning = "Averting disaster or holding on to a crumbling structure. Let truth fall where it must.",
                Description = "A monolithic stone tower struck by divine lightning, crown dislodged, amidst falling astral sparks."
            },
            new() {
                Id = 17, Name = "The Star", Arcana = "Major", Number = 17, Element = "Air", AstrologicalSign = "Aquarius",
                Symbol = "⭐", Keywords = "Hope, Cosmic Inspiration, Renewal, Starlight",
                UprightMeaning = "Peace enters your spirit. The cosmic waters refresh your weary soul with boundless hope.",
                ReversedMeaning = "Despair or feeling adrift from your guiding star. Look up; the light has never faded.",
                Description = "A serene maiden pouring luminous starlight onto sacred waters under eight radiant stars."
            },
            new() {
                Id = 18, Name = "The Moon", Arcana = "Major", Number = 18, Element = "Water", AstrologicalSign = "Pisces",
                Symbol = "🌙", Keywords = "Mystery, Dreams, Shadow Realm, Psychic Flow",
                UprightMeaning = "Traverse the hypnotic lunar landscape. Heed your dreams; not everything is as it appears by daylight.",
                ReversedMeaning = "Confusion dissipates as the fog parts. Unveiling deceptive appearances.",
                Description = "A full moon shining upon a winding path flanked by a dog, a wolf, and a crayfish rising from water."
            },
            new() {
                Id = 19, Name = "The Sun", Arcana = "Major", Number = 19, Element = "Fire", AstrologicalSign = "Sun",
                Symbol = "☀️", Keywords = "Radiance, Joy, Vitality, Supreme Success",
                UprightMeaning = "Unadulterated triumph and golden warmth! Your endeavors are blessed with vitality and pure joy.",
                ReversedMeaning = "Temporary clouded skies or delayed celebration. The sun remains behind the clouds.",
                Description = "A joyous child riding a white horse beneath a towering, radiant golden sun and blooming sunflowers."
            },
            new() {
                Id = 20, Name = "Judgement", Arcana = "Major", Number = 20, Element = "Fire", AstrologicalSign = "Pluto",
                Symbol = "🎺", Keywords = "Awakening, Higher Calling, Absolution, Renewal",
                UprightMeaning = "The archangel's trumpet sounds your soul's graduation. Rise to your sacred destiny.",
                ReversedMeaning = "Self-judgment, harsh remorse, or ignoring your calling out of fear.",
                Description = "The angel Gabriel summoning souls from their tombs into celestial radiance."
            },
            new() {
                Id = 21, Name = "The World", Arcana = "Major", Number = 21, Element = "Earth", AstrologicalSign = "Saturn",
                Symbol = "🌍", Keywords = "Completion, Cosmic Harmony, Wholeness, Mastery",
                UprightMeaning = "A master cycle concludes in glorious wholeness. You dance in perfect rhythm with the universe.",
                ReversedMeaning = "Near the finish line, yet a crucial lesson remains to be integrated. Persevere.",
                Description = "A dancing celestial figure enclosed in a sacred laurel wreath surrounded by the four divine guardians."
            },
            // Select Minor Arcana highlights
            new() {
                Id = 22, Name = "Ace of Cups", Arcana = "Minor", Suit = "Cups", Number = 1, Element = "Water",
                Symbol = "🏆", Keywords = "Emotional Awakening, Love, Spiritual Grace",
                UprightMeaning = "An overflowing chalice of love, empathy, and creative passion fills your heart to the brim.",
                ReversedMeaning = "Blocked emotional expression or vulnerability fatigue.",
                Description = "A divine chalice overflowing with five streams of living water."
            },
            new() {
                Id = 23, Name = "Ace of Swords", Arcana = "Minor", Suit = "Swords", Number = 1, Element = "Air",
                Symbol = "⚔️", Keywords = "Mental Breakthrough, Epiphany, Sharp Intellect",
                UprightMeaning = "A piercing sword of clarity cuts through confusion, giving birth to a brilliant revelation.",
                ReversedMeaning = "Cognitive overload or sharp words creating unnecessary rift.",
                Description = "A celestial hand emerging from clouds holding a sword crowned with laurel leaves."
            },
            new() {
                Id = 24, Name = "Ace of Wands", Arcana = "Minor", Suit = "Wands", Number = 1, Element = "Fire",
                Symbol = "🔥", Keywords = "Creative Spark, Passion, Divine Inspiration",
                UprightMeaning = "A roaring spark of passion and divine enterprise ignites your soul with unstoppable vitality.",
                ReversedMeaning = "Creative fatigue or lack of clear direction for raw energy.",
                Description = "A sprouting wand radiating living leaves amidst the celestial sky."
            },
            new() {
                Id = 25, Name = "Ace of Pentacles", Arcana = "Minor", Suit = "Pentacles", Number = 1, Element = "Earth",
                Symbol = "🪙", Keywords = "Material Blessing, Prosperity, Concrete Opportunity",
                UprightMeaning = "A tangible seed of wealth, health, and grounded security is placed in your hands.",
                ReversedMeaning = "Missed financial opportunity or poor groundwork.",
                Description = "A golden coin hovering over an archway leading into a lush garden of roses and lilies."
            }
        };
    }
}
