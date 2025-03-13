package retrieval;

import model.Request;
import model.Situation;

public record RetrievalRequest(Situation situation) implements Request {

	@Override
	public RequestType requestType() {
		return RequestType.RETRIEVAL;
	}
	
}