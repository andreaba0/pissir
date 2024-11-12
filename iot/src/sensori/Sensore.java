package sensori;

import java.io.Serializable;
import java.text.DecimalFormat;
import java.util.Locale;
import java.util.Random;

public class Sensore implements Serializable{

	private static final long serialVersionUID = 1L;
	
	// Campi
	private String idSensore;
	private String idGestoreIOT;
	private String tipo;

	private String lastMeasures = "N/A";

	/**
	 * Costruttore per inizializzare un nuovo sensore.
	 *
	 * @param idSensore    L'ID del sensore.
	 * @param idGestoreIOT L'ID del gestore IoT a cui il sensore è associato.
	 * @param tipo         Il tipo di sensore.
	 */
	public Sensore(String idSensore, String idGestoreIOT, String tipo) {
		this.idSensore = idSensore;
		this.idGestoreIOT = idGestoreIOT;
		this.tipo = tipo;
	}

	// Getter e Setter
	public String getIdSensore() {
		return idSensore;
	}

	public String getTipo() {
		return tipo;
	}

	public String getIdGestoreIOT() {
		return idGestoreIOT;
	}

	public String getLastMeasures() {
		return lastMeasures;
	}


	   /**
     * Prepara e pubblica il valore casuale di temperatura
     */
    public Double rilevaTemperatura(String weatherState) {
        // set local for format (force . instead of , for decimal format)
        Locale.setDefault(Locale.US);

        // generate a random temperature based on weather conditions
        double baseTemperature = 20.0; // temperatura base in gradi Celsius
        double temperatureVariation = 10.0; // variazione massima rispetto alla temperatura base

        // Modifica della temperatura in base alle condizioni meteorologiche
        switch (weatherState) {
            case "sole":
                baseTemperature += 5.0; // temperatura più alta con il sole
                break;
            case "pioggia":
                baseTemperature -= 5.0; // temperatura più bassa con la pioggia
                break;
            case "temporale":
                baseTemperature -= 8.0; // temperatura ancora più bassa con il temporale
                break;
        }

        double randomTemperature = baseTemperature + (new Random().nextDouble() * temperatureVariation);
        String formattedTemperature = new DecimalFormat("#.##").format(randomTemperature) + " 'C";
        lastMeasures = formattedTemperature;

        // debug
        System.out.println(this.idSensore + " : temperatura rilevata: " + formattedTemperature);

        return randomTemperature;
    }

    /**
     * Prepara e pubblica un valore casuale di umidità
     */
    public Integer rilevaUmidita(String weatherState) {
        // generate a random humidity based on weather conditions
        int baseHumidity = 40; // umidità base in percentuale
        int humidityVariation = 20; // variazione massima rispetto all'umidità base

        // Modifica dell'umidità in base alle condizioni meteorologiche
        switch (weatherState) {
            case "sole":
                baseHumidity -= 20; // umidità più bassa con il sole
                break;
            case "pioggia":
                baseHumidity += 20; // umidità più alta con la pioggia
                break;
            case "temporale":
                baseHumidity += 30; // umidità ancora più alta con il temporale
                break;
        }

        // Condizione che l'umidità non superi il 100%
        int randomHumidity = Math.min(baseHumidity + new Random().nextInt(humidityVariation), 100);
        String formattedHumidity = randomHumidity + " %";
        lastMeasures = formattedHumidity;

        // debug
        System.out.println(this.idSensore + " : umidita' rilevata: " + formattedHumidity);

        return randomHumidity;
    }



}
