package retrieval;

import java.text.ParseException;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;

import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;

import de.dfki.mycbr.core.Project;
import de.dfki.mycbr.core.casebase.Attribute;
import de.dfki.mycbr.core.casebase.Instance;
import de.dfki.mycbr.core.casebase.MultipleAttribute;
import de.dfki.mycbr.core.model.AttributeDesc;
import de.dfki.mycbr.core.retrieval.Retrieval;
import de.dfki.mycbr.core.retrieval.Retrieval.RetrievalMethod;
import de.dfki.mycbr.core.similarity.Similarity;
import de.dfki.mycbr.util.Pair;
import model.RouteOption;
import model.Situation;
import model.SituationSolutionAnswer;
import model.SituationSolutionAnswer.SolutionAnswerRootObject;
import myCBR.Recommender;

public class RetrievalHandler extends Recommender<RetrievalRequest> {
	
	private record RetrievalResponse(RouteOption reponseRoute, String plan) {}
	
	public RetrievalHandler(Project project) {
		super(project);
	}
	
	public JsonElement process(RetrievalRequest request) {
		try {
			System.out.println("\n- Started the Retrieval -");
			var response = conditionalRetrieval(request);
		
			var gson = new Gson();
			var jsonRoutestring = gson.toJsonTree(response.reponseRoute());
			
			var json = new JsonObject();
			json.addProperty("plan", response.plan());
			json.add("routeOptions", jsonRoutestring);
			var jsonRoot= new JsonObject();
			jsonRoot.add("routes", gson.toJsonTree(List.of(json)));
			return jsonRoot;

		} catch (Exception e) {
			e.printStackTrace();
			var json = new JsonObject();
			json.addProperty("error", e.getMessage());
			return json;
		}
	}
	
	public RetrievalResponse conditionalRetrieval(RetrievalRequest request) throws ParseException {
		RetrievalResponse response;
		if (request.situation().routeOptions().size() == 0) {
			Retrieval ret = new Retrieval(concept, caseBaseDestCards);
			ret.setRetrievalMethod(RetrievalMethod.RETRIEVE_SORTED);
			var orgSituation = request.situation();
			var emptyRoute = new RouteOption(0, 0, 0, null, 0);
			var adaptedSituation = new Situation(orgSituation.handTrainCardsCount(), orgSituation.destinationCardsCount(), orgSituation.availableWagons(), orgSituation.minimalAvailableWagons(), orgSituation.averageScoreOfRoutes(), List.of(emptyRoute), orgSituation.situationType());
			fillInstanceWithSituationalKnowledge(ret.getQueryInstance(), adaptedSituation);
			ret.start();
			var results = ret.getResult();
			
			String plan = results.stream().sorted((s1, s2) -> Double.compare(s2.getSecond().getValue(), s1.getSecond().getValue())).findFirst().get().getFirst().getAttForDesc(this.plan).getValueAsString();
			response = new RetrievalResponse(emptyRoute, plan);
		} else {
			Retrieval ret = new Retrieval(concept, caseBase);
			ret.setRetrievalMethod(RetrievalMethod.RETRIEVE_SORTED);
			var orgSituation = request.situation();
			var retrievalAnswer = request.situation().routeOptions().stream().map(r -> new Situation(orgSituation.handTrainCardsCount(), orgSituation.destinationCardsCount(), orgSituation.availableWagons(), orgSituation.minimalAvailableWagons(), orgSituation.averageScoreOfRoutes(), List.of(r), orgSituation.situationType())).map(situation -> {
				try {
					fillInstanceWithSituationalKnowledge(ret.getQueryInstance(), situation);
				} catch (ParseException e) {
					throw new RuntimeException(e);
				}
				routeConcept.setActiveAmalgamFct(routeConcept.getFct("RouteOption"));
				ret.start();
				List<Pair<Instance, Similarity>> result = ret.getResult();
				var best =  result.stream().sorted((s1, s2) -> Double.compare(s2.getSecond().getValue(), s1.getSecond().getValue())).findFirst().get();
				return best;
			}).toList();
			
			var sims = retrievalAnswer.stream().map(r -> r.getSecond().getValue()).toList();
			var bestSolutionIndex = sims.size() == 1 ? 0 : findBestSolutions(sims)[0];
			var plan = retrievalAnswer.get(bestSolutionIndex).getFirst().getAttForDesc(this.plan).getValueAsString();
			var route = request.situation().routeOptions().get(bestSolutionIndex);
			response = new RetrievalResponse(route, plan);
		}
		return response;
		
	}
		

	private static int[] findBestSolutions(List<Double> values) {
        if (values == null || values.size() < 2) {
            throw new IllegalArgumentException("The list must contain at least two elements.");
        }
        int maxIndex = -1;
        int secondMaxIndex = -1;
        double max = Integer.MIN_VALUE;
        double secondMax = Integer.MIN_VALUE;

        for (int i = 0; i < values.size(); i++) {
            var currentValue = values.get(i);
            if (currentValue > max) {
                secondMax = max;
                secondMaxIndex = maxIndex;
                max = currentValue;
                maxIndex = i;
            } else if (currentValue > secondMax && currentValue != max) {
                secondMax = currentValue;
                secondMaxIndex = i;
            }
        }

        return new int[] { maxIndex, secondMaxIndex };
	}

	private JsonObject transformToJson(HashMap<AttributeDesc, Attribute> instanceAttributes) {
		var json = new JsonObject();
		for(var entry : instanceAttributes.entrySet()) {
			Attribute attr = entry.getValue();
			AttributeDesc desc = entry.getKey();
			if (attr instanceof MultipleAttribute<?> routeOptionAttr) {
				for(Attribute val : routeOptionAttr.getValues()) {
					if (val instanceof Instance instance) {
						json.add(desc.getName(), transformToJson(instance.getAttributes()));
					}
				}
			} else {				
				json.addProperty(desc.getName(), attr.getValueAsString());
			}
		}
		
		return json;
	}
	
	private JsonElement getBestCasesSolutions(Retrieval retrieval) {
		var upperThreshold = retrieval.getResult().size() > 3 ? 3 : retrieval.getResult().size() - 1;
		var bestResults = retrieval.getResult().subList(0, upperThreshold);

		var solutions = new LinkedList<SituationSolutionAnswer>();
		var gson = new Gson();
		for (var bestResult : bestResults) {
		    var instance = bestResult.getFirst();
		    var attributes = instance.getAttributes();
		    var json = transformToJson(attributes);
		    json.addProperty("quality", bestResult.getSecond().getValue());
		    var solutionModel = gson.fromJson(json, SituationSolutionAnswer.class);
		    solutions.add(solutionModel);
		}

		String bestThreeCaseDebug = bestResults.stream()
				.map(c -> "Case " + c.getFirst().getName() + " has similarity of " + c.getSecond().getValue())
				.reduce((a,b) -> a + "\n" + b).orElse("No cases");
		System.out.println(bestThreeCaseDebug);
		System.out.println("(" + (retrieval.getResult().size() - 3) + " cases in output omitted)");
		return gson.toJsonTree(new SolutionAnswerRootObject(solutions));
	}
}
