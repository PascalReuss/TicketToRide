package model;

import adaptions.AdaptionRequest;
import adaptions.NewCaseRequest;
import retrieval.RetrievalRequest;

public interface Request {
	
	public enum RequestType {
	    RETRIEVAL(RetrievalRequest.class),
	    ADAPTION(AdaptionRequest.class),
	    NEWCASE(NewCaseRequest.class);

	    private final Class<? extends Request> associatedClass;

	    RequestType(Class<? extends Request> associatedClass) {
	        this.associatedClass = associatedClass;
	    }

	    public Class<? extends Request> getAssociatedClass() {
	        return associatedClass;
	    }
	}

	public record RequestImpl(RequestType requestType) implements Request {
		
	}
	
    RequestType requestType();
    
}
