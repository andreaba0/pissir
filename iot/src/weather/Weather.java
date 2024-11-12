package weather;

import java.util.Timer;
import java.util.TimerTask;
import java.util.Random;

/**
 * Questa classe rappresenta le condizioni meteorologiche in un sistema IoT simulato.
 * Fornisce la capacità di avere diversi stati del tempo e di aggiornare automaticamente
 * lo stato del tempo a intervalli regolari con una certa randomicità.
 */
public class Weather {
    private static String weatherState;
    private Timer timer;
    private Random random;

    /**
     * Costruttore della classe Weather.
     *
     * @param initialWeatherState Lo stato iniziale del tempo.
     */
    public Weather(String initialWeatherState) {
        Weather.weatherState = initialWeatherState;
        this.timer = new Timer();
        this.random = new Random();
        scheduleWeatherUpdate();
    }

    /**
     * Ottieni lo stato corrente del tempo.
     *
     * @return Lo stato corrente del tempo.
     */
    public static String getWeatherState() {
        return weatherState;
    }

    /**
     * Funzione per far partire il timer e cambiare lo stato del tempo
     */
    private void scheduleWeatherUpdate() {
        // Programma un task che cambierà lo stato del tempo con una certa randomicità
        timer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                changeWeatherState();
            }
        }, 0, (long) (1 * 60 * 1000)); // (minuti, secondi, millisecondi)
    }

    /**
     * Cambia lo stato del tempo con una certa randomicità
     */
    private void changeWeatherState() {
        double randomValue = random.nextDouble();

        if (randomValue <= 0.6) {
            // Probabilità del 60% per "sole"
            weatherState = "sole";
        } else if (randomValue <= 0.7) {
            // Probabilità del 10% per "nuvoloso" (0.6 + 0.1 = 0.7)
            weatherState = "nuvoloso";
        } else if (randomValue <= 0.9) {
            // Probabilità del 20% per "pioggia" (0.7 + 0.2 = 0.9)
            weatherState = "pioggia";
        } else {
            // Probabilità del 10% per "temporale" (0.9 + 0.1 = 1.0)
            weatherState = "temporale";
        }

        System.out.println("Stato del tempo cambiato a: " + weatherState);
        System.out.println();
    }


    /**
     * Interrompi il timer e gli aggiornamenti del tempo.
     */
    public void stopWeatherUpdates() {
        timer.cancel();
        System.out.println("Aggiornamenti del tempo interrotti.");
    }
}
