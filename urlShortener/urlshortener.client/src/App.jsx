import { useState } from "react";
import config from "../config.json";

function App() {
  const [longUrl, setLongUrl] = useState("");
  const urlShortener = `${config.urlShortenerHost}${config.shortenEndpoint}`;
  const shortenUrl = (e) => {
    e.preventDefault();

    console.log("Shortening url ...");
    fetch(urlShortener, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ longUrl }),
    })
      .then((response) => response.json())
      .then((shortUrl) => {
        //set long url to blank
        setLongUrl("");
        console.log(shortUrl);
      })
      .catch((err) => console.error(`error encountered: ${err}`));
  };

  return (
    <div className="container">
      <form onSubmit={shortenUrl}>
        <div className="row-input">
          <label htmlFor="url" className="label">
            <i className="material-icons">link</i>
            Shorten a long URL
          </label>
          <input
            type="url"
            id="url"
            name="longUrl"
            className="input-md"
            placeholder="Enter long link here"
            value={longUrl}
            onChange={(e) => setLongUrl(e.target.value)}
          />
        </div>
        <div className="row-input">
          <button type="submit" className="btn">
            Shorten URL
          </button>
        </div>
      </form>
    </div>
  );
}

export default App;
