using UnityEngine;

public class RopeController : MonoBehaviour
{
    // Assign these in the Inspector
    public Transform player1;
    public Transform player2;
    public LineRenderer lineRenderer;

    // Parameters for the rope
    public float maxDistance = 5f; // The distance at which the movement stops
    public float maxDistanceColorThreshold = 4.5f; // Distance where color starts fading
    public Color greenColor = Color.green;
    public Color redColor = Color.red;

    private float currentDistance;
    private Rigidbody2D rb1;
    private Rigidbody2D rb2;

    void Start()
    {
        // Safety check and getting Rigidbody components
        if (player1 != null)
            rb1 = player1.GetComponent<Rigidbody2D>();
        if (player2 != null)
            rb2 = player2.GetComponent<Rigidbody2D>();

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        // Initialize the Line Renderer points
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // 1. Calculate Distance
        Vector3 p1Position = player1.position;
        Vector3 p2Position = player2.position;
        currentDistance = Vector3.Distance(p1Position, p2Position);

        // 2. Update Line Renderer Visualization
        lineRenderer.SetPosition(0, p1Position);
        lineRenderer.SetPosition(1, p2Position);
        
        // 3. Update Color based on Distance
        UpdateLineColor();

        // 4. Implement Max Distance Constraint
        ApplyDistanceConstraint();
    }

    // --- Helper Methods ---

    void UpdateLineColor()
    {
        // Calculate the interpolation factor (t) for the color gradient.
        // The color starts fading from `maxDistanceColorThreshold` to `maxDistance`.
        
        float t = Mathf.InverseLerp(maxDistanceColorThreshold, maxDistance, currentDistance);
        
        // Clamp t between 0 and 1 to ensure smooth interpolation
        t = Mathf.Clamp01(t); 

        // Interpolate between green and red
        Color currentColor = Color.Lerp(greenColor, redColor, t);

        // Apply the color to both ends of the Line Renderer
        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;
    }

    void ApplyDistanceConstraint()
    {
        if (currentDistance > maxDistance)
        {
            // Calculate the direction from P1 to P2
            Vector3 direction = (player2.position - player1.position).normalized;

            // Calculate the target position for P2, exactly 'maxDistance' away from P1
            Vector3 targetP2Position = player1.position + direction * maxDistance;

            // Determine which object is pulling away (or the one to move back)
            // A simple approach is to move the object that has traveled the farthest,
            // or, more simply, move *both* back a portion of the overflow.

            // The overflow distance
            float overflow = currentDistance - maxDistance;
            
            // We want to pull them back so the distance is exactly maxDistance.
            // Move each object back by half the overflow along the direction vector.
            
            if (rb1 != null && rb2 != null)
            {
                // This is a simple position-setting solution (less realistic):
                // player2.position = targetP2Position; 
                
                // Better: Use Physics (Kinematic/Velocity) to stop/pull them back.
                // Apply a strong force *towards* the center of the rope.
                
                // Get the center point of the rope
                Vector3 centerPoint = (player1.position + player2.position) / 2f;
                
                // Direction from P1 to Center
                Vector3 p1ToCenter = (centerPoint - player1.position).normalized;
                // Direction from P2 to Center
                Vector3 p2ToCenter = (centerPoint - player2.position).normalized;

                // Apply a strong opposing force (or set velocity to zero/opposite)
                float forceMagnitude = overflow * 50f; // Tune this value!
                
                // Apply force back towards the center
                rb1.AddForce(p1ToCenter * forceMagnitude, ForceMode2D.Force);
                rb2.AddForce(p2ToCenter * forceMagnitude, ForceMode2D.Force);
                
                // If you want a complete, immediate stop:
                // rb1.velocity = Vector2.zero;
                // rb2.velocity = Vector2.zero;
            }
        }
    }
}
//