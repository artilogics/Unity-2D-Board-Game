using UnityEngine;

public class SpecialTile : MonoBehaviour
{
    public enum TileEffect
    {
        None,
        ExtraRoll,
        Shortcut,
        SkipTurn
    }

    public TileEffect effect = TileEffect.ExtraRoll;
    public System.Collections.Generic.List<Transform> possibleDestinations;
}
