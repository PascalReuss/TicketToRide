package adaptions;

import static model.Solution.ClaimRoute;
import static model.Solution.DrawCardFromDeck;

import java.util.List;

import com.google.gson.Gson;
import com.google.gson.JsonElement;

import de.dfki.mycbr.core.Project;
import model.SituationSolutionAnswer;
import model.SituationSolutionAnswer.SolutionAnswerRootObject;
import model.Solution;
import myCBR.Recommender;

public class SolutionAdapter extends Recommender<AdaptionRequest> {

	public SolutionAdapter(Project project) {
		super(project);
	}

	@Override
	public JsonElement process(AdaptionRequest context) {
		System.out.println("- Start Adaption - ");
		switch (context.adaptionType()) {
		case UNFEASABLE: {
			var updatedSolutionList = replaceSolution(context.casesToAdapt());
			return new Gson().toJsonTree(new SolutionAnswerRootObject(updatedSolutionList));
		}
		default:
			throw new UnsupportedOperationException("Not implemented" + context.adaptionType());
		}
	}

	public static List<SituationSolutionAnswer> replaceSolution(List<SituationSolutionAnswer> casesToAdapt) {
		return casesToAdapt.stream().map(situation -> {
			if (situation.plan().equals(ClaimRoute)) {
				return new SituationSolutionAnswer(DrawCardFromDeck, situation.routeOptions(), situation.quality());
			}
			return situation;
		}).toList();
	}

}
