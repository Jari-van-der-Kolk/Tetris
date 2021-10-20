  using System;
  using UnityEngine;
  using Random = UnityEngine.Random;

  public class Bag <T> 
    {
        private T[] bagContents;
        private T[] peekBagContents;
        private int remaining = 0;
        
        public Bag(T[] bagContents)
        {
            this.bagContents = bagContents;
            peekBagContents = bagContents;
        }

        public T Next()
        {
            /*if (remaining <= 0)
            {
                Shuffle();
            }*/
            remaining--;
            return bagContents[remaining];
        }
        
        public void Shuffle()  
        {
            int n = bagContents.Length;
            while (n > 1)
            {  
                n--;  
                int k = Random.Range(0, n + 1);
                (bagContents[k], bagContents[n]) = (bagContents[n], bagContents[k]);
                 peekBagContents[n] = bagContents[n];
            }

            remaining = bagContents.Length;
        }

        
        
        public T Peek()
        {
            if (remaining <= 0)
            {
                Shuffle();
            }
            return bagContents[remaining - 1];
        }
    }
