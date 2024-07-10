package adaptions;

import java.util.List;

import model.Request;
import model.SituationSolutionAnswer;

public record AdaptionRequest(AdaptionType adaptionType, List<SituationSolutionAnswer> casesToAdapt) implements Request {

	public static enum AdaptionType {
		UNFEASABLE, UPDATE;
	}

	public RequestType requestType() {
		return RequestType.ADAPTION;
	}

}