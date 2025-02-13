import { useState, useReducer } from "react";
import config from "../config.json";

function App() {
  const initialFormData = {
    longUrl: "",
    loading: false,
    done: false,
    errorMsg: "",
    shortUrl: "",
    copied: false,
  };

  const formDataReducer = (formData, action) => {
    switch (action.type) {
      case "loading": {
        return {
          ...formData,
          loading: true,
        };
      }
      case "urlShortened": {
        return {
          ...formData,
          longUrl: "",
          shortUrl: action.shortUrl,
        };
      }
      case "handleError": {
        return {
          ...formData,
          errorMsg: action.errorMsg,
        };
      }
      case "doneProcessing": {
        return {
          ...formData,
          loading: false,
          done: true,
        };
      }
      case "resetForm": {
        return initialFormData;
      }
      case "changeCopied": {
        return {
          ...formData,
          copied: action.copied,
        };
      }
      case "longUrlChanged": {
        return {
          ...formData,
          longUrl: action.longUrl,
        };
      }
      default: {
        throw new Error("Unknown action: " + action.type);
      }
    }
  };

  const [formData, dispatch] = useReducer(formDataReducer, initialFormData);

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
      .then((response) => {
        return new Promise((resolve) => {
          response.json().then((data) =>
            resolve({
              ok: response.ok,
              statusCode: response.status,
              data,
            })
          );
        });
      })
      .then(({ ok, statusCode, data }) => {
        if (!ok) {
          throw new Error(data.map((d) => d.errorMessage).join("\n"), {
            cause: statusCode,
          });
        }
        dispatch({ type: "urlShortened", shortUrl: data.shortUrl });
      })
      .catch((err) => {
        dispatch({
          type: "handleError",
          errorMsg:
            (err.cause && err.message) ||
            "Something went wrong. Please try again",
        });
      })
      .finally(() => {
        dispatch({ type: "doneProcessing" });
      });

    dispatch({ type: "loading" });
  };

  const copyLink = () => {
    navigator.clipboard.writeText(shortUrl).then(() => {
      dispatch({ type: "changeCopied", copied: true });
      setTimeout(() => {
        dispatch({ type: "changeCopied", copied: false });
      }, 800);
    });
  };

  const { longUrl, loading, done, errorMsg, shortUrl, copied } = formData;

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
              onChange={(e) =>
                dispatch({ type: "longUrlChanged", longUrl: e.target.value })
              }
              disabled={loading}
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
                    {(copied && <span className="green-text">check</span>) ||
                      "content_copy"}
                  </i>
                  <span className="tooltiptext">
                    {(copied && "Copied") || "Copy"}
                  </span>
                </button>
              </div>
            ))}
        </div>

        <div className="row-input">
          {loading && (
            <div id="spinner" className="fa-2x">
              <i className="fa fa-spinner fa-spin"></i>
            </div>
          )}
          {(!done && !loading && (
            <button
              type="submit"
              className="btn"
              id="submit-btn"
              disabled={longUrl === ""}
            >
              {config.shortenUrlBtnText}
            </button>
          )) ||
            (done && (
              <input
                type="reset"
                onClick={() => dispatch({ type: "resetForm" })}
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
