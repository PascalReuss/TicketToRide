package myCBR;

import java.text.ParseException;
import java.util.LinkedList;
import java.util.UUID;
import java.util.stream.Collectors;

import com.google.gson.JsonElement;

import de.dfki.mycbr.core.DefaultCaseBase;
import de.dfki.mycbr.core.Project;
import de.dfki.mycbr.core.casebase.Attribute;
import de.dfki.mycbr.core.casebase.Instance;
import de.dfki.mycbr.core.casebase.MultipleAttribute;
import de.dfki.mycbr.core.model.Concept;
import de.dfki.mycbr.core.model.ConceptDesc;
import de.dfki.mycbr.core.model.FloatDesc;
import de.dfki.mycbr.core.model.IntegerDesc;
import de.dfki.mycbr.core.model.StringDesc;
import model.Request;
import model.RouteOption;
import model.Situation;


public abstract class Recommender <T extends Request>{
	

	
//    private final CBREngine engine;
//    private final Project project;
    protected final DefaultCaseBase caseBase;
    protected final DefaultCaseBase caseBaseDestCards;
    protected final Concept concept;
    protected final Concept routeConcept;

    protected final IntegerDesc handTrainCardsCount;
    protected final IntegerDesc destinationCardsCount;
    protected final IntegerDesc availableWagons;
    protected final IntegerDesc minimalAvailableWagons;
    protected final FloatDesc averageScoreOfRoutes;
    protected final ConceptDesc routeOptions;
    protected final IntegerDesc destinationCardPoints;
    protected final IntegerDesc faceUpCards;
    protected final IntegerDesc lengthOfRoute;
    protected final IntegerDesc trainCards;
    protected final StringDesc nameOfRoute;
    protected final StringDesc plan;
	
    public Recommender(Project project) {
    	assert project != null;
//        engine = new CBREngine();
//        project = engine.createProjectFromPRJ();
        caseBase = (DefaultCaseBase) project.getCaseBases().get(CBREngine.getCaseBase());
        caseBaseDestCards = (DefaultCaseBase) project.getCaseBases().get("CaseBaseDestCards");
        concept = project.getConceptByID(CBREngine.getConceptName());
        routeConcept = project.getConceptByID("routeOptions");

        handTrainCardsCount = (IntegerDesc) concept.getAllAttributeDescs().get("handTrainCardsCount");
        destinationCardsCount = (IntegerDesc) concept.getAllAttributeDescs().get("destinationCardsCount");
        availableWagons = (IntegerDesc) concept.getAllAttributeDescs().get("availableWagons");
        minimalAvailableWagons = (IntegerDesc) concept.getAllAttributeDescs().get("minimalAvailableWagons");
        averageScoreOfRoutes = (FloatDesc) concept.getAllAttributeDescs().get("averageScoreOfRoutes");

        routeOptions = (ConceptDesc) concept.getAllAttributeDescs().get("routeOptions");

        destinationCardPoints = (IntegerDesc) routeConcept.getAllAttributeDescs().get("destinationCardPoints");
        faceUpCards = (IntegerDesc) routeConcept.getAllAttributeDescs().get("faceUpCards");
        lengthOfRoute = (IntegerDesc) routeConcept.getAllAttributeDescs().get("lengthOfRoute");
        trainCards = (IntegerDesc) routeConcept.getAllAttributeDescs().get("trainCards");
        nameOfRoute = (StringDesc) routeConcept.getAllAttributeDescs().get("nameOfRoute");

        plan = (StringDesc) concept.getAllAttributeDescs().get("plan");
    }

	protected void fillInstanceWithSituationalKnowledge(Instance instance, Situation situation) throws ParseException {
		instance.addAttribute(handTrainCardsCount, handTrainCardsCount.getAttribute(situation.handTrainCardsCount()));
		instance.addAttribute(destinationCardsCount, destinationCardsCount.getAttribute(situation.destinationCardsCount()));
		instance.addAttribute(availableWagons, availableWagons.getAttribute(situation.availableWagons()));
		instance.addAttribute(minimalAvailableWagons, minimalAvailableWagons.getAttribute(situation.minimalAvailableWagons()));
		instance.addAttribute(averageScoreOfRoutes, averageScoreOfRoutes.getAttribute(situation.averageScoreOfRoutes()));
		
		LinkedList<Attribute> optionInstances = situation.routeOptions().stream().map(this::routeOptionToInstance).collect(Collectors.toCollection(LinkedList::new));
		var desc = concept.getAllAttributeDescs().get(routeOptions.getName());
		var multipleAttributeRouteOption = new MultipleAttribute<ConceptDesc>((ConceptDesc) desc, optionInstances);	
		instance.addAttribute(routeOptions, multipleAttributeRouteOption);
	}
	
	protected Instance routeOptionToInstance(final RouteOption opt) {
		try {
			Instance routeOptionInstance = new Instance(routeOptions.getConcept(), UUID.randomUUID().toString());
			routeOptionInstance.addAttribute(destinationCardPoints, destinationCardPoints.getAttribute(opt.destinationCardPoints));
			routeOptionInstance.addAttribute(faceUpCards, faceUpCards.getAttribute(opt.faceUpCards));
			routeOptionInstance.addAttribute(lengthOfRoute, lengthOfRoute.getAttribute(opt.lengthOfRoute));
			routeOptionInstance.addAttribute(trainCards, trainCards.getAttribute(opt.trainCards));
			routeOptionInstance.addAttribute(nameOfRoute, nameOfRoute.getAttribute(opt.nameOfRoute));
			return routeOptionInstance;
		} catch (ParseException e) {
			throw new RuntimeException(e);
		}
	}
	
	public abstract JsonElement process(T recommender);
	
//	Attribute utilityDesc = caseEntry.getKey().getAttForDesc(utility);
//	int utility = Integer.parseInt(utilityDesc.getValueAsString()
	
//	public void decreaseCaseBase(DefaultCaseBase cb, int limit) {
//		while (cb.getCases().size() > limit) {
//			Boolean allCasesHavePositiveUtility = true;
//			for (Instance c : cb.getCases()) {
//				if (Integer.parseInt(c.getAttForDesc(utility).getValueAsString()) < 0) {
//					allCasesHavePositiveUtility = false;
//					break;
//				}
//			}
//			
//			if (allCasesHavePositiveUtility) {
//				System.out.println("\nNo case was deleted. All cases have positive utility!");
//				return;
//			}
//			
//			Instance caseToDelete = cb.getCases().iterator().next();
//			for (Instance c : cb.getCases()) {			
//				if (Integer.parseInt(c.getAttForDesc(utility).getValueAsString()) < 
//						Integer.parseInt(caseToDelete.getAttForDesc(utility).getValueAsString())) {
//					caseToDelete = c;
//				} else if (Integer.parseInt(c.getAttForDesc(utility).getValueAsString()) == 
//						Integer.parseInt(caseToDelete.getAttForDesc(utility).getValueAsString())) {
//					if (Math.round(Math.random()) == 1)
//						caseToDelete = c;
//				}
//			}
//			cb.removeCase(caseToDelete);
//			System.out.println("Case " + caseToDelete.getAttForDesc(plan).getValueAsString() + " (" + caseToDelete.getName() 
//				+ ") with utility of " + caseToDelete.getAttForDesc(utility).getValueAsString() + " was removed from the case base.");
//			project.save();
//		}
//	}

	// updateAttributeWeight(status);
	// tempStati.add(status);

	// temporary disabled
	/*
	 * setWeightForAttr(instance, RetrievalHelper.IS_COVERED_DESC, isCoveredAttrW);
	 * setWeightForAttr(instance, RetrievalHelper.PLAN_DESC, planAttrW);
	 * setWeightForAttr(instance, RetrievalHelper.QUALITY_DESC, qualityAttrW);
	 */

//	private double SimOfOuery(Integer noun, Integer pluralnoun, String sentenceStructure, String keyword,
//			String keyword2, String verb, String verb2, Integer caseNumber) {
//
//		// similarity of the case
//		double sim = 0;
//
//		// create a new retrieval
//		Retrieval ret = new Retrieval(myConcept, cb);
//		// specify the retrieval method
//		ret.setRetrievalMethod(RetrievalMethod.RETRIEVE_SORTED);
//		// create a query instance
//		Instance query = ret.getQueryInstance();
//
//		SymbolDesc keywordDesc = (SymbolDesc) myConcept.getAllAttributeDescs().get("Keyword");
//		SymbolDesc keyword2Desc = (SymbolDesc) myConcept.getAllAttributeDescs().get("Keyword2");
//		SymbolDesc sentenceStructureDesc = (SymbolDesc) myConcept.getAllAttributeDescs().get("SentenceStructure");
//		SymbolDesc verbDesc = (SymbolDesc) myConcept.getAllAttributeDescs().get("VerbType");
//		SymbolDesc verb2Desc = (SymbolDesc) myConcept.getAllAttributeDescs().get("VerbType");
//		IntegerDesc nomenDesc = (IntegerDesc) myConcept.getAllAttributeDescs().get("NumberNouns");
//		IntegerDesc pluralnomenDesc = (IntegerDesc) myConcept.getAllAttributeDescs().get("NumberPluralNouns");
//
//		// Insert values into the query
//		query.addAttribute(keywordDesc, keywordDesc.getAttribute(keyword));
//		query.addAttribute(keyword2Desc, keyword2Desc.getAttribute(keyword2));
//		query.addAttribute(sentenceStructureDesc, sentenceStructureDesc.getAttribute(sentenceStructure));
//		query.addAttribute(verbDesc, verbDesc.getAttribute(verb));
//		query.addAttribute(verb2Desc, verb2Desc.getAttribute(verb2));
//
//		try {
//			query.addAttribute(nomenDesc, nomenDesc.getAttribute(noun));
//		} catch (ParseException e) {
//			e.printStackTrace();
//		}
//
//		try {
//			query.addAttribute(pluralnomenDesc, pluralnomenDesc.getAttribute(pluralnoun));
//		} catch (ParseException e) {
//			e.printStackTrace();
//		}
//
//		// perform retrieval
//		ret.start();
//		// get the retrieval result
//		List<Pair<Instance, Similarity>> result = ret.getResult();
//		// get the case name
//		if (result.size() > 0) {
//			sim = result.get(caseNumber).getSecond().getValue();
//			sim = Math.round(sim * 100.0);
//			ArrayList<Hashtable<String, String>> liste = new ArrayList<Hashtable<String, String>>();
//			for (int i = 0; i < caseNumber; i++) {
//				liste.add(getAttributes(result.get(i), project.getConceptByID(CBREngine.getConceptName())));
//				System.out.println("Fall: " + liste.get(i).toString());
//			}
//		} else {
//			System.out.println("Retrieval Result is empty");
//		}
//		return sim;
//	}

	/**
	 * This method delivers a Hashtable which contains the Attributs names
	 * (Attributes of the case) combined with their respective values.
	 * 
	 * @author weber,koehler,namuth
	 * @param r       = An Instance.
	 * @param concept = A Concept
	 * @return List = List containing the Attributes of a case with their values.
	 */
//	private static Hashtable<String, String> getAttributes(Pair<Instance, Similarity> r, Concept concept) {
//
//		Hashtable<String, String> table = new Hashtable<String, String>();
//		ArrayList<String> cats = getCategories(r);
//		// Add the similarity of the case
//		table.put("Sim", String.valueOf(r.getSecond().getValue()));
//		for (String cat : cats) {
//			// Add the Attribute name and its value into the Hashtable
//			table.put(cat, r.getFirst().getAttForDesc(concept.getAllAttributeDescs().get(cat)).getValueAsString());
//		}
//		return table;
//	}

	/**
	 * This Method generates an ArrayList, which contains all Categories of a
	 * Concept.
	 * 
	 * @author weber,koehler,namuth
	 * @param r = An Instance.
	 * @return List = List containing the Attributes names.
	 */
//	private static ArrayList<String> getCategories(Pair<Instance, Similarity> r) {
//
//		ArrayList<String> cats = new ArrayList<String>();
//
//		// Read all Attributes of a Concept
//		Set<AttributeDesc> catlist = r.getFirst().getAttributes().keySet();
//
//		for (AttributeDesc cat : catlist) {
//			if (cat != null) {
//				// Add the String literals for each Attribute into the ArrayList
//				cats.add(cat.getName());
//			}
//		}
//		return cats;
//	}

	// not used 
//	public String displayAmalgamationFunctions() {
//
//		ArrayList<String> amalgam = new ArrayList<String>();
//		String listoffunctions = "Currently available Amalgamationfunctions: <br /> <br />";
//		AmalgamationFct current = myConcept.getActiveAmalgamFct();
//		System.out.println("Amalgamation Function is used = " + current.getName());
//		List<AmalgamationFct> liste = myConcept.getAvailableAmalgamFcts();
//
//		for (int i = 0; i < liste.size(); i++) {
//			System.out.println(liste.get(i).getName());
//			listoffunctions = listoffunctions + liste.get(i).getName() + "<br />";
//		}
//
//		listoffunctions = listoffunctions
//				+ (" <br /> <br /> Currently selected Amalgamationfunction: " + current.getName() + "\n");
//		listoffunctions = listoffunctions
//				+ (" <br /> <br /> Please type the name of the Amalgamationfunction to use in the "
//						+ " Field \"Amalgamationfunction\" it will be automatically used during the next retrieval");
//		System.out.println(listoffunctions);
//		return listoffunctions;
//	}
}