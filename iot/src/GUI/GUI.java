package GUI;

import javax.swing.*;
import attuatori.Attuatore;
import gestoreIOT.GestoreIOT;
import main.Main;
import sensori.Sensore;
import weather.Weather;

import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.OutputStream;
import java.io.PrintStream;

/**
 * Questa classe rappresenta l'interfaccia grafica utente per il sistema IoT.
 * Fornisce funzionalità per aggiungere/rimuovere gestori, sensori, attuatori e
 * impostare intervalli di tempo.
 */
public class GUI extends JFrame {
	private static final long serialVersionUID = 2163925721803123000L;
	
	private JComboBox<String> managerIdComboBox;
    private JComboBox<String> sensorIdComboBox;
    private JComboBox<String> actuatorIdComboBox;
    private JTextArea elementList;
    private String companyName;

    // Costruttori
    /**
     * Costruttore che inizializza la GUI con il nome dell'azienda.
     *
     * @param companyName Il nome dell'azienda.
     */
    public GUI(String companyName) {
        this.companyName = companyName;
        createAndShowGUI();
    }
   

    /**
     * Metodo che comprende tutti gli elementi grafici della GUI e 
     * i relativi Listener.
     */
    private void createAndShowGUI() {
        setTitle("Gestore Sistema IoT - " + companyName);
        setSize(1000, 700);
        setMinimumSize(new Dimension(900, 600));
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

        JPanel mainPanel = new JPanel(new GridLayout(1, 4, 15, 0));

        // Pannello per la colonna sinistra
        JPanel leftPanel = new JPanel();
        // Pannello per la colonna centrale
        JPanel centerPanel = new JPanel();
        // Pannello per la colonna destra
        JPanel rightPanel = new JPanel();
        // Pannello per la colonna del tempo
        JPanel timePanel = new JPanel();
        
        
        // colonna sinistra
        leftPanel.setLayout(new BoxLayout(leftPanel, BoxLayout.Y_AXIS));
        leftPanel.setBorder(BorderFactory.createEmptyBorder(10, 10, 10, 10));
        leftPanel.setAlignmentX(Component.LEFT_ALIGNMENT);
        
        JButton updateAppButton = createStyledButton("Aggiorna e sincronizza applicazione");

        JButton renewWaterButton = createStyledButton("Rinnova Scorte Acqua");
        JButton addGestoreButton = createStyledButton("Aggiungi Gestore");
        JButton removeGestoreButton = createStyledButton("Rimuovi Gestore selezionato");

        managerIdComboBox = new JComboBox<>();
        updateManagerComboBox();

        // Pulsanti nel pannello sinistro
        leftPanel.add(updateAppButton);
        leftPanel.add(Box.createRigidArea(new Dimension(0, 10)));
		
        
        /*
        //TEST----------------------------
        leftPanel.add(renewWaterButton);
        leftPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        leftPanel.add(addGestoreButton);
        leftPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        leftPanel.add(removeGestoreButton);
        leftPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        leftPanel.add(Box.createRigidArea(new Dimension(0, 40)));
        //TEST----------------------------
		*/
		
        
        leftPanel.add(createStyledLabel("Lista Gestori (seleziona per modificare):"));
        leftPanel.add(managerIdComboBox);
        leftPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        

        
        // colonna centrale
        centerPanel.setLayout(new BoxLayout(centerPanel, BoxLayout.Y_AXIS));
        centerPanel.setBorder(BorderFactory.createEmptyBorder(10, 20, 10, 10));
        centerPanel.setAlignmentX(Component.LEFT_ALIGNMENT);
        
        centerPanel.add(createStyledLabel("Aggiungi elementi al gestore selezionato"));
        JButton startTemperatureButton = createStyledButton("Aggiungi Sensore Temperatura");
        JButton startHumidityButton = createStyledButton("Aggiungi Sensore Umidità");
        JButton startActuatorButton = createStyledButton("Aggiungi Attuatore - Irrigatore");
        centerPanel.add(startTemperatureButton);
        centerPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        centerPanel.add(startHumidityButton);
        centerPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        centerPanel.add(startActuatorButton);

        
        // colonna destra
        rightPanel.setLayout(new BoxLayout(rightPanel, BoxLayout.Y_AXIS));
        rightPanel.setBorder(BorderFactory.createEmptyBorder(10, 10, 10, 20));
        rightPanel.setAlignmentX(Component.LEFT_ALIGNMENT);
        JButton removeElementButton = createStyledButton("Elimina elementi selezionati");

        // Combobox per gli ID dei sensori
        sensorIdComboBox = new JComboBox<>();

        // Combobox per gli ID degli attuatori
        actuatorIdComboBox = new JComboBox<>();
        
        updateSensorComboBox((String)managerIdComboBox.getSelectedItem());
        updateActuatorComboBox((String)managerIdComboBox.getSelectedItem());

        rightPanel.add(createStyledLabel("Lista Sensori (seleziona per eliminare)"));
        rightPanel.add(sensorIdComboBox);
        rightPanel.add(createStyledLabel("Lista Attuatori (seleziona per eliminare)"));
        rightPanel.add(actuatorIdComboBox);
        rightPanel.add(Box.createRigidArea(new Dimension(0, 10)));
        rightPanel.add(removeElementButton);
        
        
        
        // Colonna tempo
        timePanel.setLayout(new BoxLayout(timePanel, BoxLayout.Y_AXIS));
        timePanel.setAlignmentX(Component.LEFT_ALIGNMENT);
        timePanel.setBorder(BorderFactory.createEmptyBorder(10, 10, 10, 20));
        timePanel.add(Box.createRigidArea(new Dimension(0, 10)));
        // JTextField per inserire i secondi
        JTextField secondsSensorTextField = createStyledTextField();
        JTextField secondsActuatorTextField = createStyledTextField();
        JTextField amountWaterTextField = createStyledTextField();

        timePanel.add(createStyledLabel("Frequenza misure sensori (10 secondi default):"));
        timePanel.add(secondsSensorTextField);
        timePanel.add(createStyledLabel("Intervallo spegnimento attuatori (5 secondi default):"));
        timePanel.add(secondsActuatorTextField);
        timePanel.add(createStyledLabel("Consumo attuatori (0.5 litro/secondo default):"));
        timePanel.add(amountWaterTextField);
        timePanel.add(Box.createRigidArea(new Dimension(0, 10)));
        JButton applyButton = createStyledButton("Applica");
        timePanel.add(applyButton);


        // Aggiungi colonne al pannello principale
        mainPanel.add(leftPanel);
        mainPanel.add(centerPanel);
        mainPanel.add(rightPanel);
        mainPanel.add(timePanel);

        // Aggiungi alla finestra principale
        add(mainPanel, BorderLayout.NORTH);
        
        
        // Creazione dell'area di output per la console
        JTextArea consoleOutputArea = new JTextArea(10, 30);
        consoleOutputArea.setEditable(false);
        //consoleOutputArea.setHighlighter(null);
        JScrollPane consoleScrollPane = new JScrollPane(consoleOutputArea);

        // Creazione dell'area per la lista di elementi
        elementList = new JTextArea(20, 30);
        elementList.setEditable(false);
        elementList.setHighlighter(null);
        JScrollPane elementListScrollPane = new JScrollPane(elementList);

        // Creazione di uno split pane con orientamento orizzontale
        JSplitPane splitPane = new JSplitPane(JSplitPane.HORIZONTAL_SPLIT, consoleScrollPane, elementListScrollPane);
        splitPane.setResizeWeight(0.9); // Imposta il 90% dello spazio per la console

        // Aggiungi lo split pane alla GUI
        add(splitPane, BorderLayout.CENTER);

        // Reindirizza System.out alla tua JTextArea
        PrintStream printStream = new PrintStream(new CustomOutputStream(consoleOutputArea));
        System.setOut(printStream);
        

        //LISTENER
        // elementi pannello di sinistra
        updateAppButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                Main.updateApplication();
                
                // Aggiorna la GUI
                updateElementList();
            }
        });
        
        renewWaterButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                Main.rinnovaScorteAcquaTot();
                
                // Aggiorna la GUI
                updateElementList();
            }
        });

        addGestoreButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
            	//TODO nome coltivazione
                Main.aggiungiGestore("Coltivazione 1", 100.0f);

                // Aggiorna la GUI
                updateManagerComboBox();
                updateSensorComboBox((String)managerIdComboBox.getSelectedItem());
                updateActuatorComboBox((String)managerIdComboBox.getSelectedItem());
                updateElementList();
            }
        });
        
        removeGestoreButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                String selectedManagerId = (String) managerIdComboBox.getSelectedItem();

                Main.rimuoviGestore(selectedManagerId);

                // Aggiorna la GUI
                updateManagerComboBox();
                updateSensorComboBox(selectedManagerId);
                updateActuatorComboBox(selectedManagerId);
                
                updateElementList();
            }
        });
        
        managerIdComboBox.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                // Ottieni l'id del gestore selezionato
                String selectedManagerId = (String) managerIdComboBox.getSelectedItem();

                // Aggiorna le JComboBox di attuatori e sensori in base al gestore selezionato
                updateSensorComboBox(selectedManagerId);
                updateActuatorComboBox(selectedManagerId);
            }
        });
        
        
        
        // elementi pannello centrale
        startTemperatureButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                String selectedManagerId = (String) managerIdComboBox.getSelectedItem();
                
                // Trova il gestore IOT con l'id selezionato
                GestoreIOT selectedGestore = findGestoreById(selectedManagerId);

                if (selectedGestore != null  && selectedManagerId.compareTo("Seleziona gestore") != 0) {
                    // Aggiungi il sensore al gestore IOT trovato
                    selectedGestore.aggiungiSensore("temperatura");
                    updateSensorComboBox(selectedManagerId);
                    updateElementList();
                } else {
                    System.out.println("Gestore non trovato con l'id: " + selectedManagerId);
                }
            }
        });

        startHumidityButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                String selectedManagerId = (String) managerIdComboBox.getSelectedItem();

                // Trova il gestore IOT con l'id selezionato
                GestoreIOT selectedGestore = findGestoreById(selectedManagerId);

                if (selectedGestore != null  && selectedManagerId.compareTo("Seleziona gestore") != 0) {
                    // Aggiungi il sensore al gestore IOT trovato
                    selectedGestore.aggiungiSensore("umidita");
                    updateSensorComboBox(selectedManagerId);
                    updateElementList();
                } else {
                    System.out.println("Gestore non trovato con l'id: " + selectedManagerId);
                }
            }
        });
        
        startActuatorButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                String selectedManagerId = (String) managerIdComboBox.getSelectedItem();

                // Trova il gestore IOT con l'id selezionato
                GestoreIOT selectedGestore = findGestoreById(selectedManagerId);

                if (selectedGestore != null && selectedManagerId.compareTo("Seleziona gestore") != 0) {
                    // Aggiungi l'attuatore al gestore IOT trovato
                    selectedGestore.aggiungiAttuatore("irrigatore");
                    updateActuatorComboBox(selectedManagerId);
                    updateElementList();
                } else {
                    System.out.println("Gestore non trovato con l'id: " + selectedManagerId);
                }
            }
        });

        
        // elementi pannello di destra
        removeElementButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                String selectedManagerId = (String) managerIdComboBox.getSelectedItem();
                String selectedSensorId = (String) sensorIdComboBox.getSelectedItem();
                String selectedActuatorId = (String) actuatorIdComboBox.getSelectedItem();
                
                if (selectedManagerId != null && selectedManagerId.compareTo("Seleziona gestore") != 0) {
                    GestoreIOT selectedGestore = findGestoreById(selectedManagerId);

                    if (selectedSensorId != null && selectedSensorId.compareTo("Seleziona sensore") != 0) {
                        selectedGestore.rimuoviSensore(selectedSensorId);
                        updateSensorComboBox(selectedManagerId);
                    }

                    if (selectedActuatorId != null && selectedActuatorId.compareTo("Seleziona attuatore") != 0) {
                        selectedGestore.rimuoviAttuatore(selectedActuatorId);
                        updateActuatorComboBox(selectedManagerId);
                    }

                    updateElementList();
                } else {
                    System.out.println("Seleziona un gestore prima di rimuovere un elemento.");
                }
            }
        });
        
        // elementi pannello tempo
        applyButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                try {
                    // Valori di default
                    int intervalloSensori = 10;
                    int intervalloAttuatori = 5;
                    float consumoAcquaAttuatori = 1.0f;

                    // Recupera i valori dalle caselle di testo
                    if (secondsSensorTextField.getText() != null && !secondsSensorTextField.getText().isEmpty()) {
                        intervalloSensori = Integer.parseInt(secondsSensorTextField.getText());
                    }
                    if (secondsActuatorTextField.getText() != null && !secondsActuatorTextField.getText().isEmpty()) {
                        intervalloAttuatori = Integer.parseInt(secondsActuatorTextField.getText());
                    }
                    if (amountWaterTextField.getText() != null && !amountWaterTextField.getText().isEmpty()) {
                        consumoAcquaAttuatori = Float.parseFloat(amountWaterTextField.getText());
                    }

                    // Applica gli intervalli ai sensori e agli attuatori
                    Main.modificaIntervalloSensori(intervalloSensori);
                    Main.modificaIntervalloAttuatori(intervalloAttuatori);
                    Main.modificaConsumoAttuatori(consumoAcquaAttuatori);

                    System.out.println("Intervallo dei sensori e degli attuatori modificato.\n");

                } catch (NumberFormatException ex) {
                    System.out.println("Wrong numeric value inserted for interval.\n");
                }
            }
        });

        
        //rendo visibile la finestra
        setExtendedState(JFrame.MAXIMIZED_BOTH);
        setVisible(true);
        
        // inizializzo la lista elementi
        updateElementList();
    }
    
    
    
    //METODI 
    /**
     * Metodo per trovare un oggetto gestore IOT dato l'ID.
     *
     * @param gestoreId L'ID del gestore IOT.
     * @return Il gestore IOT corrispondente, o null se non trovato.
     */
    private GestoreIOT findGestoreById(String gestoreId) {
        for (GestoreIOT gestore : Main.getGestori()) {
            if (gestore.getIdGestoreIoT().equals(gestoreId)) {
                return gestore;
            }
        }
        return null;
    }
    
    /**
     * Classe personalizzata per sovrascrivere l'OutputStream di System.out.
     */
    private class CustomOutputStream extends OutputStream {
        private JTextArea textArea;

        public CustomOutputStream(JTextArea textArea) {
            this.textArea = textArea;
        }

        @Override
        public void write(int b) {
            // Scrive il byte sulla JTextArea
            textArea.append(String.valueOf((char) b));
            // Scrolla automaticamente alla fine del testo
            textArea.setCaretPosition(textArea.getDocument().getLength());
            updateElementList();
        }
    }

    
    // Metodo per creare pulsanti con dimensioni uniformi
    private JButton createStyledButton(String text) {
        JButton button = new JButton(text);
        Dimension buttonSize = new Dimension(300, 30);
        button.setMaximumSize(buttonSize);
        button.setAlignmentX(Component.LEFT_ALIGNMENT);
        return button;
    }
    // Metodo per creare caselle di testo con dimensioni uniformi
    private JTextField createStyledTextField() {
        JTextField textField = new JTextField();
        Dimension fieldSize = new Dimension(300, 30);
        textField.setMaximumSize(fieldSize);
        textField.setAlignmentX(Component.LEFT_ALIGNMENT);
        return textField;
    }
    // Metodo per creare label con dimensioni uniformi
    private JLabel createStyledLabel(String text) {
        JLabel label = new JLabel(text);
        Dimension labelSize = new Dimension(300, 30);
        label.setMaximumSize(labelSize);
        label.setAlignmentX(Component.LEFT_ALIGNMENT);
        return label;
    }

    
    /*
     * Aggiorna la JComboBox dei gestori IOT
     */
    public void updateManagerComboBox() {
        managerIdComboBox.removeAllItems();
        managerIdComboBox.addItem("Seleziona gestore");
        for (GestoreIOT gestore : Main.getGestori()) {
            managerIdComboBox.addItem(gestore.getIdGestoreIoT());
        }
    }
    
    /**
     * Aggiorna dinamicamente la JComboBox dei sensori in base al gestore selezionato.
     *
     * @param selectedManagerId L'ID del gestore selezionato.
     */
    private void updateSensorComboBox(String selectedManagerId) {
        sensorIdComboBox.removeAllItems();
        sensorIdComboBox.addItem("Seleziona sensore");
        
        if (selectedManagerId != null && selectedManagerId.compareTo("Seleziona gestore")!=0) {
            // Seleziona il gestore corrispondente
            GestoreIOT selectedGestore = findGestoreById(selectedManagerId);
            
            if (selectedGestore != null) {
                // Aggiungi gli ID dei sensori del gestore corrispondente
            	for (Sensore sensore : selectedGestore.getSensori()) {
                    sensorIdComboBox.addItem(sensore.getIdSensore());
                }
			}
            
        }
    }

    /**
     * Aggiorna dinamicamente la JComboBox degli attuatori in base al gestore selezionato.
     *
     * @param selectedManagerId L'ID del gestore selezionato.
     */
    private void updateActuatorComboBox(String selectedManagerId) {
        actuatorIdComboBox.removeAllItems();
        actuatorIdComboBox.addItem("Seleziona attuatore");
        
        if (selectedManagerId != null && selectedManagerId.compareTo("Seleziona gestore")!=0) {
            // Seleziona il gestore corrispondente
            GestoreIOT selectedGestore = findGestoreById(selectedManagerId);
            
            if (selectedGestore != null) {
            	// Aggiungi gli ID degli attuatori del gestore corrispondente
                for (Attuatore attuatore : selectedGestore.getAttuatori()) {
                    actuatorIdComboBox.addItem(attuatore.getIdAttuatore());
                }
            }
            
        }
    }
    

    /**
     * Aggiorna la lista totale degli elementi visualizzati nella GUI.
     */
    public void updateElementList() {
        StringBuilder elementListText = new StringBuilder();
        if (Main.getGestori().size() != 0) {
        	elementListText.append("-- <Dati applicazione> -----------------------------------\n");

        	elementListText.append("Azienda P.IVA: "+Main.getCompanyName()+"\n");
        	//elementListText.append("Chiave: " + Main.getCompanySecret()+"\n");
        	elementListText.append("Tempo: " + Weather.getWeatherState()+"\n\n\n");

        	elementListText.append("-- <Lista Gestori (Coltivazioni)> ----------------------\n");
            for (GestoreIOT gestoreIOT : Main.getGestori()) {
                elementListText.append("\n-------- <Gestore "+gestoreIOT.getIdGestoreIoT()+"> -------------\n");
                elementListText.append("--- ID Coltivazione: "+gestoreIOT.getIdColtivazione()+"\n");
                elementListText.append("--- Risorse idriche restanti: "+gestoreIOT.getScorteAcqua()+" litri\n");
                elementListText.append("--- Intervallo misurazione sensori: "+gestoreIOT.getIntervalloSensori()+" secondi\n");
                elementListText.append("--- Intervallo spegnimento attuatori: "+gestoreIOT.getIntervalloAttuatore()+" secondi\n");
                elementListText.append("--- Risorse erogate attuatori: "+gestoreIOT.getConsumoAcquaAttuatore()+" litri/secondo\n");

                elementListText.append("- Sensori:\n");
                for (Sensore sensore : gestoreIOT.getSensori()) {
                    elementListText.append("    ").append(sensore.getIdSensore()).append(" - Ultima misura: "+ sensore.getLastMeasures() +"\n");
                }
                elementListText.append("    ").append("Ultima media temperature: "+ gestoreIOT.getLastMediaT()+"\n");
                elementListText.append("    ").append("Ultima media umidita': "+ gestoreIOT.getLastMediaU()+"\n");
                elementListText.append("- Attuatori:\n");
                for (Attuatore attuatore : gestoreIOT.getAttuatori()) {
                    elementListText.append("    ").append(attuatore.getIdAttuatore()).append(" - ").append(attuatore.getStato()).append("\n");
                }
                elementListText.append("\n\n");
            }
        } else {
        	elementListText.append("---Dati applicazione---\nTempo: " + Weather.getWeatherState()+"\n\n");
            elementListText.append("Nessun gestore presente\n");
        }

        elementList.setText(elementListText.toString());
        elementList.repaint();
    }

}
