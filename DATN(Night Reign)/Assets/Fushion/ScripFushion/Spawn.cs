using Fusion;
using UnityEngine;
//class spawn player
public class Spawn : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerFre;
    public void PlayerJoined(PlayerRef player)
    {
        if(player == Runner.LocalPlayer)
        { 
            var position = new Vector3(0.447555542f, 0.940999985f, 13.4908943f);
            Runner.Spawn(PlayerFre,
                        position,
                        Quaternion.identity,
                        Runner.LocalPlayer, (runner, Obj) =>
                        {
                            var playerSetup = Obj.GetComponent<PlayerSetup>();
                            if (playerSetup != null)
                            {
                                playerSetup.SetUpCam();
                            }
                        });
        }
    }
}
