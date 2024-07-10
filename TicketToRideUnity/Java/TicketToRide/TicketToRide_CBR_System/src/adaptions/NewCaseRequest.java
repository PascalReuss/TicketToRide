package adaptions;

import model.Case;
import model.Request;
import model.Situation;

public record NewCaseRequest(Situation situation, Case newCase) implements Request {
	
	public RequestType requestType() {
		return RequestType.NEWCASE;
	}
	

}