namespace ManzaTools.Interfaces;

public interface IPracticeBaseService: IBaseService
{
    bool GameModeIsPractice { get; }

    bool GameModeIsPracticeMatch { get; }
}
