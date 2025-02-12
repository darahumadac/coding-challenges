import { useState } from "react";
import config from "../config.json";

function App() {
  const [longUrl, setLongUrl] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [done, setDone] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");
  const [shortUrl, setShortUrl] = useState("");
  const [isCopied, setIsCopied] = useState(false);

  const urlShortener = `${config.urlShortenerHost}${config.shortenEndpoint}`;
  const shortenUrl = (e) => {
    e.preventDefault();
    fetch(urlShortener, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ longUrl }),
    })
      .then((response) => response.json())
      .then((data) => {
        setLongUrl("");
        setShortUrl(data.shortUrl);
        console.log(data);
      })
      .catch((err) => {
        setErrorMsg("Something went wrong!");
        console.error(err);
      })
      .finally(() => {
        setIsLoading(false);
        setDone(true);
      });

    setIsLoading(true);
  };

  const resetForm = () => {
    setLongUrl("");
    setIsLoading(false);
    setDone(false);
    setErrorMsg("");
    setShortUrl("");
    setIsCopied(false);
  };

  const copyLink = () => {
    navigator.clipboard.writeText(shortUrl).then(() => {
      setIsCopied(true);
      setTimeout(() => {
        setIsCopied(false);
      }, 800);
    });
  };

  return (
    <div className="container">
      <form onSubmit={shortenUrl}>
        <div className="row-input">
          <label
            htmlFor={`${(!done && "url") || (errorMsg == "" && "short-url")}`}
            className={`label ${!errorMsg || "red-text"}`}
          >
            <i className="material-icons">
              {(errorMsg == "" && "link") || "error_outline"}
            </i>
            {(!done && config.instructionLabel) ||
              (errorMsg == "" && config.copyLinkInstructionLabel) ||
              errorMsg}
          </label>
          {(!done && (
            <input
              type="url"
              id="url"
              name="longUrl"
              className="input-md"
              placeholder="Enter long link here"
              value={longUrl}
              onChange={(e) => setLongUrl(e.target.value)}
              disabled={isLoading}
            />
          )) ||
            (errorMsg == "" && (
              <div className="input-group">
                <input
                  type="url"
                  id="short-url"
                  className="input-icon"
                  value={shortUrl}
                  disabled={true}
                />
                <button
                  type="button"
                  className="icon-button tooltip"
                  onClick={copyLink}
                >
                  <i className="large material-icons">
                    {(isCopied && <span className="green-text">check</span>) ||
                      "content_copy"}
                  </i>
                  <span className="tooltiptext">
                    {(isCopied && "Copied") || "Copy"}
                  </span>
                </button>
              </div>
            ))}
        </div>

        <div className="row-input">
          {isLoading && (
            <div id="spinner" className="fa-2x">
              <i className="fa fa-spinner fa-spin"></i>
            </div>
          )}
          {(!done && !isLoading && (
            <button type="submit" className="btn" id="submit-btn">
              {config.shortenUrlBtnText}
            </button>
          )) ||
            (done && (
              <input
                type="reset"
                onClick={resetForm}
                className="btn"
                id="generate-again-btn"
                value={
                  (errorMsg == "" && config.resetText) || config.tryAgainText
                }
              />
            ))}
        </div>
      </form>
    </div>
  );
}

export default App;
