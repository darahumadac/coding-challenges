function App() {
  return (
    <div className="container">
      <form>
        <div className="row-input">
          <label htmlFor="url" className="label">
            <i className="material-icons">link</i>
            Shorten a long URL
          </label>
          <input
            type="url"
            id="url"
            name="url"
            className="input-md"
            placeholder="Enter long link here"
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
