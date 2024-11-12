package attuatori;

import java.io.Serializable;
import java.util.HashMap;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;

import gestoreIOT.GestoreIOT;

public class Attuatore implements Serializable{

	private static final long serialVersionUID = 1L;
	
	// Campi
	private String idAttuatore;
	private String idGestoreIOT;
	private String tipo;
	private String stato; // ON / OFF(default)

	private transient Timer timer;

	/**
	 * Costruttore per inizializzare un nuovo attuatore.
	 *
	 * @param idAttuatore  L'ID dell'attuatore.
	 * @param idGestoreIOT L'ID del gestore IoT a cui l'attuatore è associato.
	 * @param tipo         Il tipo di attuatore.
	 */
	public Attuatore(String idAttuatore, String idGestoreIOT, String tipo) {
		this.idAttuatore = idAttuatore;
		this.idGestoreIOT = idGestoreIOT;
		this.tipo = tipo;
		this.stato = "OFF"; // default
	}

	// Getter e Setter
	public String getIdAttuatore() {
		return idAttuatore;
	}

	public String getIdGestoreIOT() {
		return idGestoreIOT;
	}

	public String getTipo() {
		return tipo;
	}

	public String getStato() {
		return stato;
	}
	
	public void setStato(String stato) {
		this.stato = stato;
	}	

	// debug
	// System.out.println(idAttuatore + " : Messaggio pubblicato su topic '" +
	// actuatorTopic.getName() + "': " + message);



	/*
	 * Aziona l'attuatore
	 * 
	 * @param stato Lo stato dell'attuatore
	 * @param durata La durata di attivazione dell'attuatore
	 * @param acquaErogata La quantità d'acqua da erogare
	 */
	public void azioneAttuatore(GestoreIOT gestore, String stato, Float durata, Float acquaErogata) throws IllegalArgumentException{
               
        switch (stato) {
		case "ON": 
			// Annulla il timer precedente, se presente
    	    if (timer != null) {
    	        timer.cancel();
    	    }
    	    
    	    // Cambia stato attuatore
    	    this.setStato("ON");
            System.out.println(idAttuatore + " : stato attuatore impostato su ON per "+durata+ " secondi. Erogazione di "+ acquaErogata +" litri.");
            
            // Richiamo metodo del gestore per inviare l'azione dell attuatore
            //gestore.inviaDati("azioni/"+getIdAttuatore(), "ON");
            
            // Partenza timer per spegnimento attuatore
    		timer = new Timer();
            timer.schedule(new TimerTask() {
                @Override
                public void run() {
                	
                	// Creazione di una mappa per rappresentare l'azione
                    Map<String, Object> azione = new HashMap<>();
                    azione.put("status", "ON");
                    azione.put("period", durata*1000); //in ms
                    azione.put("water_used", acquaErogata);
                    
                    // Creazione della stringa JSON manualmente
                    String jsonString = "{\"status\":\"ON\","
                            + "\"period\":" + durata + ","
                            + "\"water_used\":" + acquaErogata + "}";
                    
                    gestore.inviaDati("/actuator", jsonString, idAttuatore);

                    setStato("OFF");
                    System.out.println(idAttuatore + " : stato attuatore impostato su OFF");

                    // Richiamo metodo del gestore per inviare l'azione dell attuatore
                    //gestore.inviaDati("azioni/"+getIdAttuatore(), gestore.mapToJsonString(azioneOff));
                }
            }, (long) (durata*1000)); // s in ms
			break;
		
		case "OFF":
			this.setStato("OFF");
			System.out.println(idAttuatore + " : stato attuatore impostato su OFF");
			
			// Richiamo metodo del gestore per inviare l'azione dell attuatore
            //gestore.inviaDati("azioni/"+getIdAttuatore(), "OFF");
			break;
		
		default:
			throw new IllegalArgumentException("Unexpected value: " + stato);
        }
			
	}
	
}
