using FortuneTeller.Share.Common;
using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Domain.Features.AiReading;

public class AiReadingService
{
    public (string CoreProphecy, string DetailedInterpretation, string CosmicAdvice, string SacredAffirmation)
        GenerateReading(
            ReadingType type,
            ReadingTopic topic,
            string? question,
            List<SelectedCardModel>? cards,
            PalmReadingModel? palmAnalysis)
    {
        var questionContext = !string.IsNullOrWhiteSpace(question)
            ? $"Concerning your inquiry: \"{question.Trim()}\""
            : "Concerning the unfolding path before your soul";

        if (type == ReadingType.Tarot && cards != null && cards.Count > 0)
        {
            return SynthesizeTarotReading(topic, questionContext, cards);
        }
        else if (type == ReadingType.Palm && palmAnalysis != null)
        {
            return SynthesizePalmReading(topic, questionContext, palmAnalysis);
        }

        return (
            "The cosmic currents are gathering.",
            "The celestial sphere reflects profound alignment awaiting your command.",
            "Center your breath and align your sovereign intention.",
            "I am one with the infinite cosmic harmony."
        );
    }

    private static (string, string, string, string) SynthesizeTarotReading(
        ReadingTopic topic,
        string questionContext,
        List<SelectedCardModel> cards)
    {
        var primaryCard = cards[0];
        var cardSummary = string.Join(" ➔ ", cards.Select(c =>
            $"[{c.PositionName}: {c.Card.Name}{(c.IsReversed ? " (Reversed)" : "")}]"));

        string coreProphecy;
        string detailed;
        string advice;
        string affirmation;

        switch (topic)
        {
            case ReadingTopic.Love:
                coreProphecy = $"A celestial awakening is transmuting your emotional sanctuary through the vibration of {primaryCard.Card.Name}.";
                detailed = $"{questionContext}, the sacred cards reveal a profound emotional opening: {cardSummary}. " +
                           $"The presence of {primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) indicates that old relational defenses are dissolving into a higher octaval union. " +
                           (cards.Count > 1
                               ? $"The dynamic unfolds through {cards[1].Card.Name}, demanding authenticity, before culminating into {cards.Last().Card.Name}'s promise of enduring harmony."
                               : "Step forward with an open, sovereign heart.");
                advice = "Release all desire to control the timing of love. Radiate the frequency of the beloved you wish to attract.";
                affirmation = "I am a sacred sanctuary of divine love; I give and receive affection with sovereign grace.";
                break;

            case ReadingTopic.Education:
                coreProphecy = $"The Temple of Wisdom unlocks its golden sanctum through the revelation of {primaryCard.Card.Name}.";
                detailed = $"{questionContext}, the astral spread weaves a luminous scholastic trajectory: {cardSummary}. " +
                           $"With {primaryCard.Card.Name} presiding ({primaryCard.RevealedMeaning}), your intellect is primed to pierce through dense complexities and synthesize profound mastery. " +
                           (cards.Count > 1
                               ? $"Overcome temporary intellectual doubts indicated by {cards[1].Card.Name}, and anchor the breakthrough promised by {cards.Last().Card.Name}."
                               : "Your mind is sharp as an astral diamond.");
                advice = "Commit to rigorous discipline paired with contemplative rest. True scholarship requires both the fire of action and the water of reflection.";
                affirmation = "Divine intelligence flows through my mind; every lesson unfolds into absolute mastery.";
                break;

            case ReadingTopic.Relationship:
                coreProphecy = $"Karmic threads are untangling into golden chords of mutual honor under {primaryCard.Card.Name}.";
                detailed = $"{questionContext}, the divine mirrors reflect the truth of your alliances: {cardSummary}. " +
                           $"The guidance of {primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) reminds you that relationships are sacred catalysts for soul evolution. " +
                           (cards.Count > 1
                               ? $"Address unspoken tensions through {cards[1].Card.Name}, leading directly to the reciprocal equilibrium of {cards.Last().Card.Name}."
                               : "Honor both your boundaries and your capacity for compassionate empathy.");
                advice = "Speak your truth without venom, and listen without defense. Sacred equilibrium is born from mutual vulnerability.";
                affirmation = "I cultivate sacred alliances rooted in reciprocal truth, honor, and celestial harmony.";
                break;

            case ReadingTopic.General:
            default:
                coreProphecy = $"The Great Cosmic Wheel turns in your favor, spearheaded by the auspicious omen of {primaryCard.Card.Name}.";
                detailed = $"{questionContext}, the celestial cards unfurl your destiny's tapestry: {cardSummary}. " +
                           $"The primordial resonance of {primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) announces that a pivotal life cycle is shifting. " +
                           (cards.Count > 1
                               ? $"Navigate the threshold marked by {cards[1].Card.Name}, keeping your eyes fixed upon the triumphant horizon of {cards.Last().Card.Name}."
                               : "Cosmic forces stand ready to support your courageous steps forward.");
                advice = "Do not mistake stillness for stagnation. Beneath the surface, the universe is rearranging circumstances for your highest elevation.";
                affirmation = "I walk with unwavering courage upon the path carved for my highest celestial destiny.";
                break;
        }

        return (coreProphecy, detailed, advice, affirmation);
    }

    private static (string, string, string, string) SynthesizePalmReading(
        ReadingTopic topic,
        string questionContext,
        PalmReadingModel palm)
    {
        var core = $"Your {palm.HandArchetype} radiates celestial resonance with a {palm.LineClarityPercentage}% clarity alignment.";
        var detailed = $"{questionContext}, the biometric contours of your palm reveal profound cosmic signatures. " +
                       $"{palm.ElementalDescription} " +
                       $"The heart line reveals: {palm.HeartLineInterpretation} " +
                       $"The head line indicates: {palm.HeadLineInterpretation} " +
                       $"The life line confirms: {palm.LifeLineInterpretation} " +
                       $"The mounts declare: {palm.MountsAnalysis} " +
                       $"Sacred discovery: {palm.DivineSymbolFound}.";

        var advice = $"Nurture your primary energetic nexus ({palm.PrimaryEnergyCenter}). Ground your physical vessel daily to channel this high-frequency vitality.";
        var affirmation = $"My hands weave miracles, my lines channel divine life-force, and my destiny shines eternally.";

        return (core, detailed, advice, affirmation);
    }
}
