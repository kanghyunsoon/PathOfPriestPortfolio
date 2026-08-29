using UnityEngine;

public class Render_Dungeon : MonoBehaviour
{
    public Speech_Character[] characters;

    public void init()
    {
        for(int i = 0; i< characters.Length; i++)
        {
            characters[i].Init(false);
        }
    }
 }
