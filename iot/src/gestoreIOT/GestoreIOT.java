package gestoreIOT;

import java.io.Serializable;
import java.nio.charset.StandardCharsets;
import java.text.DecimalFormat;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import org.eclipse.paho.client.mqttv3.*;

import attuatori.Attuatore;
import main.Main;
import sensori.Sensore;
import weather.Weather;

public class GestoreIOT implements MqttCallback, Serializable {
	
	// CAMPI
	private String idGestoreIoT;
	private String idColtivazione;
	private List<Sensore> sensori;
	private List<Attuatore> attuatori;
	private Integer countSensori = 0;
	private Integer countAttuatori = 0;
	private String topicBackend;

	private Float scorteAcqua;
	private Integer intervalloSensori;
	private Float intervalloAttuatore;
	private Float consumoAcquaAttuatore;

	private static String BROKER_URI = "tcp://192.168.178.180:1883";
	private transient MqttClient client;
	private transient Timer timer;

	private static Double MAX_TEMPERATURE = 25.0d;
	private static Double MIN_HUMIDITY = 40.0d;
	
	private String lastMediaT = "N/A";
	private String lastMediaU = "N/A";
	
	private static final long serialVersionUID = 1L;

	/**
	 * Costruttore per la classe GestoreIOT.
	 *
	 * @param idGestoreIoT   L'ID del gestore IoT.
	 * @param idColtivazione L'ID della coltivazione associata al gestore IoT.
	 */
	public GestoreIOT(String idGestoreIoT, String idColtivazione) {
		this.idGestoreIoT = idGestoreIoT;
		this.idColtivazione = idColtivazione;
		this.sensori = new ArrayList<>();
		this.attuatori = new ArrayList<>();

		topicBackend = "$share/IOT/backend/info/" + idGestoreIoT + "/#";

		this.scorteAcqua = 100.0f; // TODO da prendere in input
		this.intervalloSensori = 10; // secondi per default
		this.intervalloAttuatore = 5.0f; // secondi per default
		this.consumoAcquaAttuatore = 0.5f; // litri al secondo

		// Inizializzazione del client MQTT
		try {
			client = new MqttClient(BROKER_URI, MqttClient.generateClientId());
			this.start();
		} catch (MqttException e) {
			e.printStackTrace();
		}
	}

	/**
	 * Costruttore per la classe GestoreIOT basato su dati JSON.
	 *
	 * @param idGestoreIoT   L'ID del gestore IoT.
	 * @param idColtivazione L'ID della coltivazione associata al gestore IoT.
	 * @param sensori        Elenco dei sensori.
	 * @param attuatori      Elenco degli attuatori.
	 * @param scorteAcqua    Le scorte d'acqua.
	 */
	public GestoreIOT(String idGestoreIoT, String idColtivazione, List<Sensore> sensori, List<Attuatore> attuatori,
			Float scorteAcqua) {
		this.idGestoreIoT = idGestoreIoT;
		this.idColtivazione = idColtivazione;
		this.sensori = sensori;
		this.attuatori = attuatori;
		this.scorteAcqua = scorteAcqua;

		// Inizializzazione del client MQTT
		try {
			client = new MqttClient(BROKER_URI, MqttClient.generateClientId());
			this.start();
		} catch (MqttException e) {
			e.printStackTrace();
		}
	}

	// Getter e Setter
	public String getIdGestoreIoT() {
		return idGestoreIoT;
	}

	public List<Sensore> getSensori() {
		return sensori;
	}

	public List<Attuatore> getAttuatori() {
		return attuatori;
	}

	public Float getScorteAcqua() {
		return scorteAcqua;
	}

	public String getIdColtivazione() {
		return idColtivazione;
	}

	public void setScorteAcqua(Float scorteAcqua) {
		this.scorteAcqua = scorteAcqua;
	}

	public Float getIntervalloAttuatore() {
		return intervalloAttuatore;
	}

	public void setIntervalloAttuatore(Float intervalloAttuatore) {
		this.intervalloAttuatore = intervalloAttuatore;
	}

	public Float getConsumoAcquaAttuatore() {
		return consumoAcquaAttuatore;
	}

	public void setConsumoAcquaAttuatore(Float consumoAcquaAttuatore) {
		this.consumoAcquaAttuatore = consumoAcquaAttuatore;
	}

	public Integer getIntervalloSensori() {
		return intervalloSensori;
	}

	public void setIntervalloSensori(Integer intervalloSensori) {
		this.intervalloSensori = intervalloSensori;
	}
	
	
	public String getLastMediaT() {
		return lastMediaT;
	}

	public String getLastMediaU() {
		return lastMediaU;
	}

	/**
	 * Metodo per aggiungere un sensore al gestore IoT.
	 *
	 * @param tipo Il tipo di sensore da aggiungere.
	 */
	public void aggiungiSensore(String tipo) {
		// idSensore build from M(idGestoreIOT)A(count Sensore)
		// example M1A0 --> manager id 1 sensor id 0
		String idSensore = this.idGestoreIoT + "-S" + this.countSensori;
		if (tipo == "temperatura") {
			idSensore += "T";
		}
		if (tipo == "umidita") {
			idSensore += "U";
		}
		this.countSensori++;
		Sensore sensore = new Sensore(idSensore, idGestoreIoT, tipo);
		this.sensori.add(sensore);
		
		/*
		// Invia il messaggio della creazione di un nuovo sensore al backend
		if (tipo.compareTo("temperatura")==0) {
			inviaDati("add/sensore/tmp", idSensore);
		}
		else if(tipo.compareTo("umidita")==0) {
			inviaDati("add/sensore/umdty", idSensore);
		}
		*/
		
		avviaMisurazioni(intervalloSensori);
	}

	/**
	 * Metodo per aggiungere un attuatore al gestore IoT.
	 *
	 * @param tipo Il tipo di attuatore da aggiungere.
	 */
	public void aggiungiAttuatore(String tipo) {
		// idAttuatore build from M(idGestoreIOT)A(count Attuatori)
		// example M1A2 --> manager id 1 actuator id 2
		String idAttuatore = this.idGestoreIoT + "-A" + this.countAttuatori;
		this.countAttuatori++;
		Attuatore attuatore = new Attuatore(idAttuatore, idGestoreIoT, tipo);
		this.attuatori.add(attuatore);
		
		// Invia il messaggio della creazione di un nuovo sensore al backend
		//inviaDati("add/attuatore", idAttuatore);
	}

	/**
	 * Metodo per rimuovere un sensore dal gestore IoT.
	 *
	 * @param idSensore L'ID del sensore da rimuovere.
	 */
	public void rimuoviSensore(String idSensore) {
		Iterator<Sensore> iterator = this.sensori.iterator();
		while (iterator.hasNext()) {
			Sensore sensore = iterator.next();
			if (sensore.getIdSensore().equals(idSensore)) {
				iterator.remove();
				System.out.println(idGestoreIoT + ": sensore rimosso: " + idSensore);
				
				// Invia il messaggio di rimozione del sensore
				//inviaDati("remove/sensore/"+sensore.getIdSensore(), sensore.getIdSensore());
				
				return;
			}
		}
		avviaMisurazioni(intervalloSensori);
		System.out.println(idGestoreIoT + ": nessun sensore trovato con ID: " + idSensore);
	}

	/**
	 * Metodo per rimuovere un attuatore dal gestore IoT.
	 *
	 * @param idAttuatore L'ID dell'attuatore da rimuovere.
	 */
	public void rimuoviAttuatore(String idAttuatore) {
		Iterator<Attuatore> iterator = this.attuatori.iterator();
		while (iterator.hasNext()) {
			Attuatore attuatore = iterator.next();
			if (attuatore.getIdAttuatore().equals(idAttuatore)) {
				iterator.remove();
				System.out.println(idGestoreIoT + ": attuatore rimosso: " + idAttuatore);
				
				// Invia il messaggio di rimozione del sensore
				//inviaDati("remove/attuatore/"+attuatore.getIdAttuatore(), attuatore.getIdAttuatore());
				
				return;
			}
		}
		System.out.println(idGestoreIoT + ": nessun attuatore trovato con ID: " + idAttuatore);
	}

	/**
	 * Metodo per rimuovere tutti i sensori e attuatori gestiti dal gestore IoT.
	 */
	public void rimuoviTutto() {
		sensori.clear();
		System.out.println(idGestoreIoT + ": tutti i sensori rimossi.");

		attuatori.clear();
		System.out.println(idGestoreIoT + ": tutti gli attuatori rimossi.");
	}

	/**
	 * Metodo per avviare il subscriber. Ascolta tutti i topic relativi alle misure,
	 * alle azioni degli attuatori e al backend.
	 */
	public void start() {
		try {
			// Inizializzazione del client MQTT
			try {
				client = new MqttClient(BROKER_URI, MqttClient.generateClientId());
			} catch (MqttException e) {
				e.printStackTrace();
			}
			
			String password = "pissir2023";
			char pwd[] = password.toCharArray();
			MqttConnectOptions options = new MqttConnectOptions();
			options.setUserName("pissir");
			options.setPassword(pwd);
			options.setCleanSession(false);
			options.setWill(client.getTopic("home/LWT"), "GestoreIOT: I'm gone. Bye.".getBytes(), 0, false);

			// set a callback and connect to the broker
			client.setCallback(this);
			client.connect(options);

			client.subscribe(topicBackend);

			avviaMisurazioni(intervalloSensori);

			System.out.println();
			System.out.println("Il gestore " + idGestoreIoT + " e' in ascolto su " + topicBackend + "...");
			System.out.println();

		} catch (MqttException e) {
			e.printStackTrace();
		}

	}

	
	
	
	// GESTIONE SENSORI E ATTUATORI
	/*
	 * Effettua misurazioni periodicamente e decide se attivare o meno gli attuatori
	 */
	private void avviaMisurazioni(int interval) {
		
		if (timer != null) {
			timer.cancel();
		}
		
		if (sensori.isEmpty()) {
			return;
		}
		
		timer = new Timer();
		timer.schedule(new TimerTask() {
			@Override
			public void run() {

				Double sommaTemperatura = 0.0d;
				Integer sommaUmidita = 0;
				Double mediaTemperatura;
				Double mediaUmidita;
				Integer countT = 0;
				Integer countU = 0;

				// Avvio misurazioni per ogni sensore
				for (Sensore sensore : sensori) {
					if (sensore.getTipo().compareTo("temperatura") == 0) {
						
						// Rileva temperature
						Double m = sensore.rilevaTemperatura(Weather.getWeatherState());
						sommaTemperatura += m;
						countT++;
						
						// Invio dati al backend
						DecimalFormat df = new DecimalFormat("#.##");
						String message = df.format(m);
			            
			            // Creazione della stringa JSON manualmente
			            String jsonString = "{\"value\":" + message + "}";
			            //String jsonString = "{\"temperature\":" + message + ",\"time\":"+ System.currentTimeMillis() +"}";

						inviaDati("/tmp", jsonString, sensore.getIdSensore());
						
					} else if (sensore.getTipo().compareTo("umidita") == 0) {
						
						// Rileva umidità
						Integer m = sensore.rilevaUmidita(Weather.getWeatherState());
						sommaUmidita += m;
						countU++;
						
						// Creazione della stringa JSON manualmente
			            String jsonString = "{\"value\":" + m.toString() + "}";
			            //String jsonString = "{\"humidity\":" + m.toString() + ",\"time\":"+ System.currentTimeMillis() +"}";

						// Invio dati al backend
						inviaDati("/umdty", jsonString, sensore.getIdSensore());
						
					} else {
						System.out.println("Tipo sensore errato.");
					}
				}
				
				
				// Calcolo medie temperature/umidità e controllo soglia per campo
				if (countT != 0) {
					mediaTemperatura = sommaTemperatura/countT;
					
					// Stringa per l'ultima media
					DecimalFormat df = new DecimalFormat("#.##");
					lastMediaT = df.format(mediaTemperatura) + " 'C";
					
					// Controllo per attivazione attuatori
					System.out.printf("%s: media misurazioni temperatura: %.2f 'C%n", idGestoreIoT, mediaTemperatura);
					if (mediaTemperatura > MAX_TEMPERATURE) {
						updateResources();
					}
					System.out.println();				

				}
				
				if (countU != 0) {
					mediaUmidita = (double) (sommaUmidita/countU);
					
					// Stringa per l'ultima media
					lastMediaU = mediaUmidita+" %";

					// Controllo per attivazione attuatori
					System.out.println(idGestoreIoT+": media misurazioni umidita': "+mediaUmidita+" %");
					if (mediaUmidita < MIN_HUMIDITY) {
						updateResources();
					}
					System.out.println();				

				}
			}
		}, 0, interval * 1000); // s in ms
	}

	/*
	 * Metodo per aggiornare l'intervallo del timer dei sensori
	 */
	public void updateSensorInterval(int newInterval) {
		// Cancella il timer attuale
		timer.cancel();

		// Imposta il nuovo intervallo
		this.intervalloSensori = newInterval;

		// Crea un nuovo timer con l'intervallo aggiornato
		avviaMisurazioni(newInterval);
	}
	
	/**
	 * Controllo del tempo, attivazione degli attuatori e aggiornamento risorse
	 * idriche
	 */
	private void updateResources() {
		
		if (attuatori.isEmpty()) {
			System.out.println(idGestoreIoT+": nessun attuatore disponibile.");
			return;
		}
		
		// Estrazione stringa del tempo
		String tempo = Weather.getWeatherState();

		// Controlla le condizioni meteorologiche prima di attivare gli attuatori
		if ("pioggia".equals(tempo) || "temporale".equals(tempo)) {
			System.out.println(idGestoreIoT + ": condizioni meteorologiche avverse, non attivare gli attuatori.\n");
			return;
		}

		// Aggiorna le risorse idriche
		float acquaErogata = consumoAcquaAttuatore * intervalloAttuatore;
		float diffAcqua = scorteAcqua - acquaErogata;
		if (diffAcqua >= 0.0f) {
			this.eseguiAzioneAttuatori("ON", acquaErogata);
			
			// Creazione di una mappa per rappresentare l'azione
            //Map<String, Object> azione = new HashMap<>();
            //azione.put("status", "ON");
            //azione.put("period", intervalloAttuatore);
            //azione.put("water_used", consumoAcquaAttuatore);
            
            // Invio dati in JSon
			//inviaDati("azioniAttuatori/", mapToJsonString(azione));
			
			scorteAcqua -= acquaErogata;
		} else if (scorteAcqua != 0) {
			System.out.println(idGestoreIoT + ": impossibile erogare tutta l'acqua necessaria.");
			System.out.println("Erogate le rimanenti risorse idriche: " + scorteAcqua + "\n");
			this.eseguiAzioneAttuatori("ON", scorteAcqua);
			
			// Creazione di una mappa per rappresentare l'azione
            //Map<String, Object> azione = new HashMap<>();
            //azione.put("status", "ON");
            //azione.put("period", intervalloAttuatore);
            //azione.put("water_used", scorteAcqua);
			
            // Invio dati in JSon
			//inviaDati("azioniAttuatori/", mapToJsonString(azione));
			scorteAcqua = 0.0f;
		} else { // attivo attuatore ma non eroga nulla
			System.out.println(idGestoreIoT + ": impossibile attivare gli attuatori. Risorse mancanti!\n");
			this.eseguiAzioneAttuatori("ON", 0);

			// Creazione di una mappa per rappresentare l'azione
            //Map<String, Object> azione = new HashMap<>();
            //azione.put("status", "ON");
            //azione.put("period", intervalloAttuatore);
            //azione.put("water_used", 0);
            
            // Invio dati in JSon
			//inviaDati("azioniAttuatori/", mapToJsonString(azione));
		}
	}

	/**
	 * Metodo per eseguire un'azione su tutti gli attuatori gestiti dal gestore IoT
	 *
	 * @param message Il messaggio da inviare agli attuatori.
	 */
	private void eseguiAzioneAttuatori(String stato, float acqua) {
		// Attiva gli attuatori
		for (Attuatore attuatore : this.attuatori) {
			try {
				attuatore.azioneAttuatore(this, stato, intervalloAttuatore, acqua/attuatori.size());
			} catch (Exception e) {
				e.printStackTrace();
			}

		}
	}
		
	
	
	
	
	
	
	/**
	 * Metodo per inviare dati al backend
	 *
	 * @param topicParziale Topic su cui scrivere il messaggio al beckend
	 * @param messageData       Messaggio inviato dai sensori/attuatori
	 */
	public void inviaDati(String topicParziale, String messageData, String objid) {

		// create topic for a single manager IOT and related device
		String topicString = "backend/measure" + topicParziale;

		// get the topic
		MqttTopic managerTopic = client.getTopic(topicString);

		// Costruzione stringa JSON log data: {type: <sensor/actuator>, vat_number: <>, obj_id: <>, data: <>, log_timestamp: <>}
		String message = "{\"type\":";
		if (topicParziale=="/actuator") {
			message+="\"actuator\",";
		}
		else {
			message+="\"sensor\",";
		}
		message+="\"vat_number\":"+"\""+Main.getCompanyName()+"\",";
		message+="\"obj_id\":"+"\""+objid+"\",";
		message+="\"data\":"+"\""+messageData+"\",";
		message+="\"log_timestamp\":"+System.currentTimeMillis();
		message+="\"field_id\":"+"\""+this.idColtivazione+"\"";
		message+="}";
		String encoded = Main.base64UrlEncode(message);
		String messageEncoded = encoded;
		messageEncoded += ".";
		messageEncoded+=Main.base64UrlEncode(Main.calculateHmacSHA256(message, Main.getCompanySecret()));

		// codifica messaggio
		//String messageEncoded = Main.base64UrlEncode(Main.calculateHmacSHA256(message, Main.getCompanySecret()));
		
		// publish the message on the given topic
		// by default, the QoS is 1 and the message is not retained
		try {
			//json+HMAC
			managerTopic.publish(new MqttMessage((message+messageEncoded).getBytes()));
		} catch (MqttException e) {
			e.printStackTrace();
		}

		// debug
		System.out.println(idGestoreIoT + " : messaggio pubblicato su topic '" + managerTopic.getName() + "': " + message);
	}	
	
	
	
	// METODI DI OVERRIDE DELLA CALLBACK MQTT
	@Override
	public void connectionLost(Throwable cause) {
		// what happens when the connection is lost. We could reconnect here, for
		// example.
	}

	@Override
	public void deliveryComplete(IMqttDeliveryToken token) {
		// called when delivery for a message has been completed, and all
		// acknowledgments have been received
		// no-op, here
	}

	@Override
	public void messageArrived(String topic, MqttMessage message) throws Exception {
		// what happens when a new message arrives: in this case, we print it out.
		String strmsg = new String(message.getPayload(), StandardCharsets.ISO_8859_1);
		System.out.println(idGestoreIoT + " : messaggio arrivato al gestore per il topic '" + topic + "': " + strmsg + "\n");

		if ("home/LWT".equals(topic)) {
			System.err.println(message);
			// Aggiungi qui le azioni aggiuntive per il Last Will and Testament message
			return;
		}

		//String[] componentiTopic = topic.split("/");
	
		// Controllo topic rinnovo scorte d'acqua  --> backend/info/idGestoreIOT/acqua
		if (topic.compareTo("backend/info/"+getIdGestoreIoT()+"/acqua")==0) {
			setScorteAcqua(Float.valueOf(strmsg));
		}
		
	}



}
