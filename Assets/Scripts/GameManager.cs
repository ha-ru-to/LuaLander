using UnityEngine;

public class GameManager : MonoBehaviour {

    private int score;

    private void Start() {
        Lander.Instance.OnCoinPickUp += Lander_OnCoinPickUp;
        Lander.Instance.OnLanded += Lander_OnLanded;
    }

    private void Lander_OnLanded(object sender, Lander.OnLandedEventArgs e) {
        AddScore(e.score);
    }

    private void Lander_OnCoinPickUp(object sender, System.EventArgs e) {
        AddScore(500);
    }

    public void AddScore(int addScoreAmmount) {
        score += addScoreAmmount;
        Debug.Log(score);
    }
}