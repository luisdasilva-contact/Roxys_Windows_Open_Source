using System.Collections.Generic;
using UnityEngine;

public static class ImgMaster 
{
 /*
  * Contains references to every image file in the main gameplay loop.
 */
    public static List<string> spriteTitles = new()
    {
        "neco-roxy",
        "marie",
        "neco-max",
        "roxy fat brass",
        "MarieFood1",
        "MarieFood2",
        "MarieFood3",
        "MarieFood4",
        "MarieFood5",
        "MarieFood6",
        "MarieFood7",
        "MarieFood8",
        "Thrift1",
        "Thrift2",
        "Thrift3",
        "Thrift4",
        "Thrift5",
        "Thrift6",
        "Thrift7",
        "Thrift8",
        "AC Avatar",
        "Blobert",
        "bottle",
        "BURN",
        "bus",
        "CAT",
        "CHIP",
        "commission",
        "CUTE",
        "Dalle1",
        "Dalle2",
        "Dalle3",
        "Dalle4",
        "Dates",
        "DEATH",
        "DqdEv6dz",
        "Drifting",
        "DS_logo",
        "fBrz8M-a",
        "Fuzzy",
        "goose",
        "GOT DAMN!!!",
        "Loaf",
        "manga pfp",
        "MISTY",
        "Mood",
        "No money",
        "Overwhelmed",
        "paul photo",
        "Picnic2",
        "Raisins",
        "roxy photo",
        "Scarf",
        "Souphle",
        "Sustenance",
        "WITCH",
        "z3dmcN00",
        "cocoon",
        "doodles",
        "sophie persona",
        "sophie",
        "Spiral",
        "super roxy doodle",
        "binding doodles",
        "Tortoise",
        "old_TV"
    };
    public static List<Sprite> allSprites = new() { };

    /*
     * Retrieves each image file's sprite, using the titles as a reference to retrieve them from the Resources folder. Those Sprites are then added to the allSprites list.
     */ 
    public static void InitializeAllSpritesList()
    {
        foreach (string name in spriteTitles)
        {
            Sprite newSprite = Resources.Load<Sprite>(name);
            allSprites.Add(newSprite);
        }
    }
}
