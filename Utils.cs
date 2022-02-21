using UnityEngine;

namespace Utils
{
    /*Utils are utility classes that are big and used often*/
    public class Utils
    {

    }
    
    /*Helpers is supporting code*/
    public static class Helpers
    {
        
    }
    
    /*Extensions are to extend basic features of other components*/
    public static class Extensions
    {
        public static void DeleteChildren(this Transform t)
        {
            //Deletes all children from a transform
            foreach (Transform _child in t) Object.Destroy(_child.gameObject);
        }
        
#region Translations
        public static void LerpThis(this Transform t, Vector3 targetPos, float time)
        {
            //easy way to lerp transform 
            //NOTE: Non-linear
            Vector3.Lerp(t.position, targetPos, time);
        }

        public static void MoveThis(this Transform t, Vector3 targetPos, float step)
        {
            //Linear way to lerp transform
            Vector3.MoveTowards(t.position, targetPos, step);
        }
#endregion    

#region Audio

        
        public static void RandomPitch(this AudioSource t, bool normalised)
        {
            if(normalised)
            {
                //Returns random pitch without introducing audio distortion
                Random.Range(0.7f, 1f);
            }
            else{
                //Generates random pitch within "okay" range
                Random.Range(0.8f, 1.1f);
            }
        }

        private static void RandomiseAudio(this AudioSource t, bool normalised)
        {
            if(normalised)
            {
                //Returns randomised audio without introducing audio distortion
                t.pitch = Random.Range(0.7f, 1f);
                t.volume = Random.Range(0.9f, 1f);
            }
            else{
                //Generates random pitch within "okay" range
                t.pitch = Random.Range(0.8f, 1.1f);
                t.volume = Random.Range(0.9f, 1f);
            }
        }

#endregion
       
    }
}