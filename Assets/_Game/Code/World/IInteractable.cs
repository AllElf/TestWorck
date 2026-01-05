namespace Game.World
{
    public interface IInteractable
    {
        bool CanInteract(Game.Player.PlayerController player);
        string GetHint(Game.Player.PlayerController player);
        void Interact(Game.Player.PlayerController player);
    }
}
