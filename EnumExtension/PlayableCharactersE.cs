using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReturnUnusedCharacters.EnumExtension
{
    [EnumExtension(typeof(PlayableCharacters))]
    public class PlayableCharactersE
    {
        public static PlayableCharacters Lamey;
        public static PlayableCharacters Cosmonaut;
        public static PlayableCharacters Ninja;
    }
}
