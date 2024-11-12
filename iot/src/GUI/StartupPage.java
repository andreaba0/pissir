package GUI;

import javax.swing.*;
import main.Main;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

public class StartupPage extends JFrame {

	private static final long serialVersionUID = -2609826172414127854L;
	private JTextField companyNameTextField;
	private JTextField companyKeyTextField;

    private JButton confirmButton;

    // Costruttore della classe StartupPage
    public StartupPage() {
        createAndShowStartupPage();
    }

    // Metodo per creare e mostrare la pagina di avvio
    private void createAndShowStartupPage() {
        setTitle("Gestore Sistema IoT - Login");
        setSize(400, 200);
        setMinimumSize(new Dimension(400, 250));
        setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);

        // Creazione del pannello principale
        JPanel panel = new JPanel();
        panel.setLayout(new BoxLayout(panel, BoxLayout.Y_AXIS));
        panel.setBorder(BorderFactory.createEmptyBorder(20, 20, 20, 20));

        // Creazione dei componenti della GUI
        companyNameTextField = new JTextField();
        companyNameTextField.setMaximumSize(new Dimension(Integer.MAX_VALUE, 40));
        companyKeyTextField = new JTextField();
        companyKeyTextField.setMaximumSize(new Dimension(Integer.MAX_VALUE, 40));
        confirmButton = new JButton("Conferma");
        confirmButton.setMaximumSize(new Dimension(Integer.MAX_VALUE, 40));

        // Aggiunta dei componenti al pannello
        panel.add(new JLabel("Inserisci la partita IVA dell'azienda:"));
        panel.add(Box.createRigidArea(new Dimension(0, 10)));
        panel.add(companyNameTextField);
        panel.add(new JLabel("Inserisci la chiave segreta dell'azienda:"));
        panel.add(Box.createRigidArea(new Dimension(0, 10)));
        panel.add(companyKeyTextField);
        panel.add(Box.createRigidArea(new Dimension(0, 10)));
        panel.add(confirmButton);

        // Aggiunta del listener al pulsante di conferma
        confirmButton.addActionListener(new ActionListener() {
            @Override
            public void actionPerformed(ActionEvent e) {
                // Ottieni il nome dell'azienda inserito dall'utente
                String companyName = companyNameTextField.getText();
                String companyToken = companyKeyTextField.getText();
                
                if (!companyName.isEmpty() && !companyToken.isEmpty()) {
                	// Chiudi la pagina di avvio
                    dispose();

                    // Avvia l'applicazione principale con il nome dell'azienda
                    Main.startApplication(companyName, companyToken);
                }
                else {
                	// Mostra un messaggio di errore se il campo è vuoto
                    JOptionPane.showMessageDialog(StartupPage.this, "Inserisci la partita iva e una chiave dell'azienda valide.", "Errore", JOptionPane.ERROR_MESSAGE);
                    return;
				}

                
            }
        });

        // Aggiunta del pannello alla GUI
        add(panel, BorderLayout.CENTER);

        // Impostazione della posizione della finestra al centro dello schermo
        setLocationRelativeTo(null);
        setVisible(true);
    }
}
