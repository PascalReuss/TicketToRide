package myCBR;

import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.Iterator;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;

import de.dfki.mycbr.core.DefaultCaseBase;
import de.dfki.mycbr.core.Project;
import model.Request;
import util.ArgParser;

public class CBRSystem {

	private static CBREngine engine;

	private static final Gson GSON = new Gson();

	public static void main(String[] args) throws FileNotFoundException {

		var cliArgs = new ArgParser(args);

		if (cliArgs.hasArgument("logout")) {
			System.out.println(CBREngine.data_path);
			PrintStream out = new PrintStream(new FileOutputStream(cliArgs.getArgument("logout")));
			System.setOut(out);
		}

		int port;
		if (cliArgs.hasArgument("port")) {
			port = Integer.parseInt(cliArgs.getArgument("port"));
		} else {
			port = 5555;
		}

		System.out.println("Starting server on the port " + port);
		engine = CBREngine.getInstance();

		if (cliArgs.hasArgument("single")) {
			CBRSystem.receive(port);
		} else {
			ExecutorService executor = Executors.newSingleThreadExecutor();
			executor.submit(() -> {
				while (true) {
					CBRSystem.receive(port);
				}
			});
		}
	}
	
	public static void receive(int port) {
		while (true) {

			ServerSocket serverSocket = null;
			Socket socket = null;
			InputStream in = null;
			OutputStream out = null;

			try {

				serverSocket = new ServerSocket(port);
				socket = serverSocket.accept();
				in = socket.getInputStream();
				out = socket.getOutputStream();
				String clientSentence;

				BufferedReader inFromClient = new BufferedReader(new InputStreamReader(in));
				clientSentence = inFromClient.readLine();
				
				
				var project = engine.loadMyCbrProject();
				var requestType = GSON.fromJson(clientSentence, Request.RequestImpl.class).requestType();
				var jsonParseClass = requestType.getAssociatedClass();
				var request = GSON.fromJson(clientSentence, jsonParseClass);
				System.out.println("Request is:" + request);
				Recommender recommender = RecommenderFactory.create(project, requestType);
				JsonElement answer = recommender.process(request);

				String answerJSON = GSON.toJson(answer);
				System.out.println("\nAnswer: " + answerJSON.toString());
//
				out.write(answerJSON.toString().concat("\r\n").getBytes());
				out.flush();

				System.out.println("\n-------------------------------------------------------\n");

			} catch (IOException exc) {
				System.out.println("Could not write socket stream");
				exc.printStackTrace();
			} catch (Exception e) {
				System.out.println("Exception");
				e.printStackTrace();
				var answer = new JsonObject();
				answer.addProperty("error", e.getMessage());
				try {
					out.write(GSON.toJson(answer).getBytes());
				} catch (IOException e1) {
				}
			} finally {
				try {
					out.close();
					in.close();
					socket.close();
					serverSocket.close();
					break;
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		}
	}

	/**
	 * Die Methode erzeugt die Fallbasis
	 * 
	 * @param name
	 * 
	 */
//	private static void handlePlayerCaseBase(String name) {
//		
//		if (!engine.caseBaseForPlayerAlreadyExists(name)) {
//			try {
//				//jtf.setText(jtf.getText() + "\nWir erstellen eine CaseBase f�r Spieler: " +  name);
//				engine.createCaseBaseForPlayer(name);
//				//jtf.setText(jtf.getText() + "\nCase Base erstellt!");
//				//Thread.sleep(5000);
//				engine.addDefaultCases(name);
//				//jtf.setText(jtf.getText() + "\nCases hinzugwf�gt!");
//				//Thread.sleep(5000);
//			} catch (Exception e) {
//				e.printStackTrace();
//				//jtf.setText(e.getMessage());
//			}
//		}
//	}

}
