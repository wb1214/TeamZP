using UnityEngine;

namespace Character
{
    public enum CharacterType { MHuman = 1, WHuman, MZombie, WZombie};
    public interface ICharacter
    {
        float RunSpd { get; set; }

        bool IsInvulerable { get; set; }

        void Run();
        
        void TakeDamage(int _atkType, Vector3 BitePos );
    }
}