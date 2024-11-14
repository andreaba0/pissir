package main;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.util.ArrayList;
import java.util.Base64;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import java.time.Instant;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import javax.swing.SwingUtilities;

import GUI.GUI;
import GUI.StartupPage;
import gestoreIOT.GestoreIOT;
import weather.Weather;

public class Main{

	// Lista di gestori IOT
    private static List<GestoreIOT> gestori;
    // Contatore
    private static Integer countGestori;
    // Nome dell'azienda
    private static String companyName;
    // Token dell'azienda
    private static String companySecret;

    // GUI
    private static GUI gui;
    
    // URL richieste HTTP
    private static String globalApiUrl = "http://192.168.178.180:10356/api/hydroservice/resourcemanager";
        
    /**
     * Costruttore della classe principale. Inizializza la lista dei gestori.
     */
    public Main() {
        super();
        Main.gestori = new ArrayList<>();
        countGestori = gestori.size();
    }

    
    // Getter e Setter
    public static List<GestoreIOT> getGestori() {
        return gestori;
    }
    
	public static String getCompanyName() {
		return companyName;
	}
	
	public static String getCompanySecret() {
		return companySecret;
	}
	    
    
    /**
     * Aggiunge un nuovo gestore IoT alla lista.
     *
     * @param idColtivazione L'ID della coltivazione associata al gestore.
     */
    public static void aggiungiGestore(String idColtivazione, Float scorteAcqua) {
        // idGestore costruito da GIOT(count gestore)
        // esempio GIOT1 --> gestore IOT 1
        String idGestore = companyName + "-GIOT" + countGestori;
        countGestori++;
        System.out.println("Gestore IOT aggiunto: " + idGestore);
        GestoreIOT gestoreIOT = new GestoreIOT(idGestore, idColtivazione);
        Main.gestori.add(gestoreIOT);
    }
    
    /**
     * Aggiunge un nuovo gestore IoT alla lista.
     *
     * @param gestore L'oggetto gestore.
     */
    public static void aggiungiGestore(GestoreIOT gestore) {
        countGestori++;
        System.out.println("Gestore IOT aggiunto: " + gestore.getIdGestoreIoT());
        Main.gestori.add(gestore);
    }

    /**
     * Rimuove un gestore IoT dalla lista in base all'ID.
     *
     * @param idGestore L'ID del gestore IoT da rimuovere.
     */
    public static void rimuoviGestore(String idGestore) {
        for (GestoreIOT gestoreIOT : gestori) {
            if (gestoreIOT.getIdGestoreIoT().compareTo(idGestore) == 0) {
                gestoreIOT.rimuoviTutto();
            }
        }
        Iterator<GestoreIOT> iterator = Main.gestori.iterator();
        while (iterator.hasNext()) {
            GestoreIOT gestoreIOT = iterator.next();
            if (gestoreIOT.getIdGestoreIoT().equals(idGestore)) {
                iterator.remove();
                System.out.println("Gestore IOT rimosso: " + idGestore);
                return;
            }
        }
        System.out.println("Nessun gestore trovato con ID: " + idGestore);
    }

    /**
     * Modifica l'intervallo di misurazione dei sensori per tutti i gestori.
     *
     * @param seconds L'intervallo in secondi.
     */
    public static void modificaIntervalloSensori(int seconds) {
        for (GestoreIOT gestoreIOT : gestori) {
        	gestoreIOT.updateSensorInterval(seconds);
        }
        System.out.println("Intervallo di misurazione dei sensori modificato: " + seconds + " secondi\n");
    }

    /**
     * Modifica l'intervallo di attuazione per tutti gli attuatori.
     *
     * @param seconds L'intervallo in secondi.
     */
    public static void modificaIntervalloAttuatori(int seconds) {
        for (GestoreIOT gestoreIOT : gestori) {
            gestoreIOT.setIntervalloAttuatore((float)seconds);
        }
        System.out.println("Intervallo di attuazione degli attuatori modificato: " + seconds + " secondi\n");
    }
    
    /**
     * Modifica il consumo di acqua/secondo per tutti gli attuatori.
     *
     * @param seconds L'intervallo in secondi.
     */
    public static void modificaConsumoAttuatori(float consumo) {
        for (GestoreIOT gestoreIOT : gestori) {
            gestoreIOT.setConsumoAcquaAttuatore(consumo);
        }
        System.out.println("Consumo di acqua litri/secondo degli attuatori modificato: " + consumo +" l/s\n");
    }
    
    /**
     * TEST
     * Richiede poi ogni mattina le risorse disponibili per campo --> JSON 
	 * Metodo per rinnovare le scorte d'acqua di tutti i gestori IOT
	 */
    public static void rinnovaScorteAcquaTot() {
    	Float scorteAcqua = 100.0f;
    	for (GestoreIOT gestoreIOT : gestori) {
    		gestoreIOT.setScorteAcqua(scorteAcqua);
    		System.out.println(gestoreIOT.getIdGestoreIoT()+" : scorte d'acqua rinnovate: " + scorteAcqua+" litri\n");
		}
    }
    
    /**
     * Aggiorna le informazioni dell'applicazione andando a richiedere i dati al backend
     */
    public static void updateApplication() {
        // Carica i dati serializzati al momento dell'avvio
        List<GestoreIOT> gList = caricaGestoriDaFile(companyName);
        
        // Chiamata HTTP per prendere i dati dei campi
        String jsonDatiCampi = httpGetFieldRequest();
        
        // Verifica e carica solo i campi con id presenti sia nella risposta HTTP che nel file deserializzato
        if (jsonDatiCampi != null) {
            verificaECaricaGestori(jsonDatiCampi, gList);
            
            for (GestoreIOT gestoreIOT : gestori) {
            	// Chiamata HTTP per prendere i dati delle risorse dei campi
                String jsonRisorse = httpGetWaterRequest(gestoreIOT.getIdColtivazione());
                
                // Assegna ad ogni campo il valore limit restituito dal JSON delle risorse
                assegnaRisorseAiCampi(jsonRisorse);
			}
        }
        
        // Inizializzazione combobox
        gui.updateManagerComboBox();
    }

    /**
     * Metodo chiamato dalla Startup Page per avviare l'applicazione principale.
     *
     * @param companyName Il nome dell'azienda inserito dall'utente.
     */
    @SuppressWarnings("unused")
    public static void startApplication(String companyName, String secret) {
    	// Avvia l'applicazione principale
        Main main = new Main();
        
        // Prende in input il nome dell'azienda per caricare i dati 
        // già esistenti nel database
        Main.companyName = companyName;
        Main.companySecret = secret;
        
        // Crea l'interfaccia grafica
        gui = new GUI(companyName);
        System.out.println("Programma principale avviato...");

        // Avvia la simulazione del tempo
        Weather simulazioneTempo = new Weather("temporale");

        /*
        //TEST----------------------------------
        List<GestoreIOT> gList = caricaGestoriDaFile(companyName);
        gestori = gList;
        if (gestori != null) {
            countGestori = gestori.size();
            for (GestoreIOT gestoreIOT : gestori) {
                gestoreIOT.start();
            }
            gui.updateManagerComboBox();	
		}
	    //TEST----------------------------------
        */

        // Carica i dati dell'applicazione confrontando i dati dalla richiesta HTTP ai file salvati
        // commentare per TEST
        updateApplication();
    }

    /**
     * Punto di ingresso dell'applicazione. Avvia la pagina di avvio Swing.
     *
     * @param args Gli argomenti della riga di comando.
     */
    public static void main(String[] args) {
    	Runtime.getRuntime().addShutdownHook(new Thread(() -> shutdown()));
    	
        SwingUtilities.invokeLater(() -> {
            // Avvia la pagina di avvio
            new StartupPage();
        });
    }
    
    /**
     * INVIO RICHIESTA HTTP a localhost per prendere la quantità di acqua disponibile per esso
     * il token verrà messo nella richiesta http come auth header
     * 
     * @param path Percorso risorsa dopo <ip:porta>
     * @return stringa JSon
     */
    public static String httpGetWaterRequest(String idColtivazione) {
        try {
            String apiUrl = globalApiUrl+"/water/stock/"+idColtivazione;
            URL url = new URL(apiUrl);

            HttpURLConnection connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("GET");
            
            //long iat = System.currentTimeMillis() / 1000L;
            long iat = Instant.now().getEpochSecond() - 60;
            long exp = iat + 120;
           
            String jsonReq = "{" +
            		"\"sub\":"+"\""+companyName+"\""+
                    ", \"iat\":" + iat +
                    ", \"exp\":" + exp +
                    ", \"path\":"+"\"/resourcemanager/water/stock/"+idColtivazione+"\""+
                    ", \"method\":\"GET\""+
                    "}";
            
            // Calcolo dell'HMAC-SHA256 e Base64URL encoding
            //<base64url(id_campo)>.<base64url({request_epoch:<epoch>})>.<base64url(signature)>
            
            String base64URLPayload = base64UrlEncode(jsonReq);
            
            String hmacSignature = calculateHmacSHA256(base64URLPayload, companySecret);
            String base64URLSignature = base64UrlEncode(hmacSignature);
            System.out.println(base64URLPayload+"."+base64URLSignature);
                        
            // Imposta l'header di autenticazione con il token
            connection.setRequestProperty("Authorization", "Pissir-farm-hmac-sha256 " + base64URLPayload+"."+base64URLSignature);

            // Leggi la risposta
            StringBuilder response = new StringBuilder();
            try (BufferedReader reader = new BufferedReader(new InputStreamReader(connection.getInputStream()))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    response.append(line);
                }
            }
            catch (Exception e) {
            	System.out.println("Errore nell'aggiornamento dei dati. Endpoint: "+ e.getMessage());
            	if (connection != null) {
                    try (BufferedReader errorReader = new BufferedReader(new InputStreamReader(connection.getErrorStream()))) {
                        String line;
                        while ((line = errorReader.readLine()) != null) {
                            response.append(line);
                        }
                    } catch (Exception errorStreamException) {
                        System.out.println("Error reading error stream: " + errorStreamException.getMessage());
                    }
                }
            	System.out.println(response);
			}

            // Chiudi la connessione
            connection.disconnect();

            // Restituisci la risposta come stringa JSON
            return response.toString();

        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
    
    /**
     * INVIO RICHIESTA HTTP a localhost per prendere i campi dell'azienda agricola
     * il token verrà messo nella richiesta http come auth header
     * 
     * @return stringa JSon
     */
    public static String httpGetFieldRequest() {
        try {
            String apiUrl = globalApiUrl+"/field";
            URL url = new URL(apiUrl);

            HttpURLConnection connection = (HttpURLConnection) url.openConnection();
            connection.setRequestMethod("GET");

            //long iat = System.currentTimeMillis() / 1000L;
            long iat = Instant.now().getEpochSecond() - 60;
            System.out.println(Instant.now().getEpochSecond());
            System.out.println(System.currentTimeMillis()/1000L);
            System.out.println(iat);
            long exp = iat + 120;

            // Costruzione della stringa in formato JSON
            String jsonReq = "{" +
            		"\"sub\":"+"\""+companyName+"\""+
                    ", \"iat\":" + iat +
                    ", \"exp\":" + exp +
                    ", \"path\":"+"\"/resourcemanager/field\""+
                    ", \"method\":\"GET\""+
                    "}";
            
            // Calcolo dell'HMAC-SHA256 e Base64URL encoding
            //<base64url(partita_iva)>.<base64url({request_epoch:<epoch>})>.<base64url(signature)>
            String base64URLPiva = base64UrlEncode(companyName);
            String base64URLPayload = base64UrlEncode(jsonReq);
            
            String hmacSignature = calculateHmacSHA256(base64URLPayload, companySecret);
            System.out.println(hmacSignature);
            String base64URLSignature = base64UrlEncode(hmacSignature);
            
            System.out.println(base64URLPayload+"."+base64URLSignature);
                        
            // Imposta l'header di autenticazione con il token
            connection.setRequestProperty("Authorization", "Pissir-farm-hmac-sha256 " + base64URLPayload+"."+base64URLSignature);

            // Leggi la risposta
            StringBuilder response = new StringBuilder();
            try (BufferedReader reader = new BufferedReader(new InputStreamReader(connection.getInputStream()))) {
                System.out.println("Loading");
            	String line;
                while ((line = reader.readLine()) != null) {
                    response.append(line);
                }
            }
            catch (Exception e) {
            	System.out.println("Errore nell'aggiornamento dei dati. Endpoint: "+ e.getMessage());
			}

            // Chiudi la connessione
            connection.disconnect();

            // Restituisci la risposta come stringa JSON
            return response.toString();

        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
    
    
    public static String calculateHmacSHA256(String data, String key) {
        try {
            Mac sha256Hmac = Mac.getInstance("HmacSHA256");
            SecretKeySpec secretKey = new SecretKeySpec(key.getBytes(StandardCharsets.UTF_8), "HmacSHA256");
            sha256Hmac.init(secretKey);

            byte[] hmacBytes = sha256Hmac.doFinal(data.getBytes(StandardCharsets.UTF_8));
            //return Base64.getEncoder().encodeToString(hmacBytes);
            StringBuilder hexString = new StringBuilder();
            for (byte b : hmacBytes) {
                hexString.append(String.format("%02x", b));
            }
            return hexString.toString();
        } catch (NoSuchAlgorithmException | InvalidKeyException e) {
            e.printStackTrace();
            return null;
        }
    }

    public static String base64UrlEncode(String input) {
        byte[] base64Encoded = Base64.getEncoder().encode(input.getBytes(StandardCharsets.UTF_8));
        // URL-safe encoding
        return new String(base64Encoded, StandardCharsets.UTF_8)
                .replace('+', '-')
                .replace('/', '_')
                .replace("=", "");
    }
    
    /**
     * Salva la lista di gestori in un file.
     *
     * @param fileName Il nome del file in cui salvare i dati.
     */
    private static void salvaGestori(String fileName) {
        try (ObjectOutputStream oos = new ObjectOutputStream(new FileOutputStream(fileName))) {
            oos.writeObject(gestori);
            System.out.println("Dati salvati con successo in: " + fileName);
        } catch (IOException e) {
            e.printStackTrace();
            System.err.println("Errore durante il salvataggio dei dati.");
        }
    }
    
    /**
     * Metodo da chiamare alla chiusura del programma.
     */
    private static void shutdown() {
    	if (companyName!=null) {
    		salvaGestori("data/"+companyName+".ser");
            System.out.println("Programma terminato. Dati salvati.");
		}
    }
    
    /**
     * Carica i gestori dai dati serializzati al momento dell'avvio.
     *
     * @param partitaIVA La partita IVA dell'azienda.
     */
    public static List<GestoreIOT> caricaGestoriDaFile(String partitaIVA) {
        String nomeFile = "data/"+partitaIVA + ".ser";

        try (FileInputStream fileInput = new FileInputStream(nomeFile);
             ObjectInputStream objectInput = new ObjectInputStream(fileInput)) {

            // Leggi l'oggetto dalla serializzazione
			@SuppressWarnings("unchecked")
			List<GestoreIOT> gestoriCaricati = (List<GestoreIOT>) objectInput.readObject();
            System.out.println("Dati caricati con successo da: " + nomeFile);

            return gestoriCaricati;

        } catch (IOException | ClassNotFoundException e) {
            System.out.println("Impossibile caricare i dati da: " + nomeFile);
            return null;
        }
    }
    
    /**
     * Verifica e carica solo i gestori con idColtivazione presente nella risposta HTTP.
     *
     * @param jsonDatiCampi Il JSON ricevuto dalla richiesta HTTP.
     */
    private static void verificaECaricaGestori(String jsonDatiCampi, List<GestoreIOT> gestoriCaricatiFile) {
    	// Effettua il parsing del JSON della risposta HTTP
        // e ottieni una lista di ID dei campi
        List<String> idCampiFromHTTP = parseJsonAndGetFieldIds(jsonDatiCampi);

        // Confronto richiesta HTTP e dati già presenti in app
        for (String idCampoHTTP : idCampiFromHTTP) {
            boolean gestorePresente = false;

            // Verifica se il gestore è già presente nella lista caricata dal file
            for (GestoreIOT gestoreFromFile : gestoriCaricatiFile) {
                if (idCampoHTTP.equals(gestoreFromFile.getIdColtivazione())) {
                    gestorePresente = true;
                    aggiungiGestore(gestoreFromFile);
                    break;
                }
            }

            // Se il gestore non è presente, creane uno nuovo e aggiungilo
            if (!gestorePresente) {
            	// Valore di default - richiesta acqua disponibile in un secondo momento
                aggiungiGestore(idCampoHTTP, 0.0f);
            }
        }
        
        countGestori = gestori.size();
        
        // Avvio di ogni gestore per ogni campo
        for (GestoreIOT gestoreIOT : gestori) {
            gestoreIOT.start();
        }
        
    }

    /**
     * Effettua il parsing del JSON della risposta HTTP e restituisce la lista di ID dei campi.
     *
     * @param jsonDatiCampi Il JSON ricevuto dalla richiesta HTTP.
     * @return La lista di ID dei campi ottenuta dal parsing del JSON.
     */
    private static List<String> parseJsonAndGetFieldIds(String jsonDatiCampi) {
        List<String> fieldIds = new ArrayList<>();

        // Utilizza espressioni regolari per trovare tutti gli ID dei campi nel JSON
        Pattern pattern = Pattern.compile("\"id\"\\s*:\\s*\"([^\"]+)\"");
        Matcher matcher = pattern.matcher(jsonDatiCampi);

        while (matcher.find()) {
            String idCampo = matcher.group(1);
            fieldIds.add(idCampo);
        }

        return fieldIds;
    }
    
    /**
     * Assegna il valore limit ad ogni campo in base al JSON delle risorse.
     *
     * @param jsonRisorse Il JSON ricevuto dalla richiesta HTTP per le risorse.
     */
    private static void assegnaRisorseAiCampi(String jsonRisorse) {
    	// Creazione di una mappa per memorizzare le risorse in base all'id del campo
        Map<String, Float> risorseMap = parseJsonAndGetRisorseMap(jsonRisorse);

        // Assegna il valore limit ad ogni campo
        for (GestoreIOT gestoreIOT : gestori) {
            String idCampo = gestoreIOT.getIdColtivazione();
            if (risorseMap.containsKey(idCampo)) {
                float risorse = risorseMap.get(idCampo);
                gestoreIOT.setScorteAcqua(risorse);
                System.out.println("Campo: " + idCampo + ", Risorse: " + risorse);
            }
        }
    }
    
    /**
     * Effettua il parsing del JSON delle risorse e restituisce la mappa di risorse per i campi.
     *
     * @param jsonRisorse Il JSON ricevuto dalla richiesta HTTP per le risorse.
     * @return La mappa di risorse per i campi ottenuta dal parsing del JSON.
     */
    private static Map<String, Float> parseJsonAndGetRisorseMap(String jsonRisorse) {
        Map<String, Float> risorseMap = new HashMap<>();

        // Utilizziamo una regex per estrarre le coppie chiave-valore dall'array JSON
        Pattern pattern = Pattern.compile("\\{\"field_id\":\"(.*?)\",\"limit\":(\\d*\\.?\\d+).*?\\}");
        Matcher matcher = pattern.matcher(jsonRisorse);

        // Iteriamo sulle corrispondenze trovate
        while (matcher.find()) {
            String idCampo = matcher.group(1);
            float limit = Float.parseFloat(matcher.group(2));
            risorseMap.put(idCampo, limit);
        }

        return risorseMap;
    }
    
    
    
}
