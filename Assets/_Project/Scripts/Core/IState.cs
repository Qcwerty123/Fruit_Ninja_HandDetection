using UnityEngine;

public interface IState
{
    void Enter();
    void Tick(); // Dùng thay cho Update
    void Exit();
}